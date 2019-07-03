using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const string HighScoreKey = "high_score";

    // Game Object
    public PlayerController PlayerTank;
    public ColliderEventObserver[] Wiper;
    public EnemySpawner[] EnemySpawners;
    public EnemySpawner BonusSpawner { get { return EnemySpawners[3]; } }
    public Transform CloudTransform;

    // UI
    public GameObject StartButton;
    public Text CurrentScoreText;
    public Text HighScoreText;
    public Text CurrentLevelText;
    public Image LevelRemainingTimeGauge;

    // Game Parameter
    public ReactiveProperty<int> GameLevel = new ReactiveProperty<int>(0);
    public float RemainingGameTime = 0;
    public bool InGame { get { return RemainingGameTime > 0; } }

    private int remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
    private List<VehicleController> spawnedEnemy = new List<VehicleController>();
    private ReactiveProperty<int> currentScore = new ReactiveProperty<int>(0);
    private ReactiveProperty<int> highScore = new ReactiveProperty<int>(0);

    private void Awake()
    {
        Instance = this;

        Wiper.ToList().ForEach(x => x.ObserveOnTriggerEnter().Subscribe(OnObjectEnterWiper).AddTo(this));
        PlayerTank.ObserveOnDead().Subscribe(_ => OnPlayerDead());
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);
        currentScore.Subscribe(x => CurrentScoreText.text = string.Format("Score:{0}", x));
        highScore.Subscribe(x => HighScoreText.text = string.Format("High  :{0}", x));
        GameLevel.Subscribe(x => CurrentLevelText.text = string.Format("{0} / {1}", x, LevelDesign.LevelMax));

        highScore.Value = PlayerPrefs.GetInt(HighScoreKey);
    }

    private void Update()
    {
        LevelRemainingTimeGauge.fillAmount = RemainingGameTime / LevelDesign.GameTime;
        CloudTransform.Rotate(Vector3.up, 0.1f);
        if (!InGame)
        {
            return;
        }

        if ((RemainingGameTime -= Time.deltaTime) <= 0)
        {
            StartCoroutine(StageEnd());
        }
    }

    public void OnGameStartButton()
    {
        TouchManager.Instance.ObserveOnTap().Subscribe(_ => PlayerTank.Fire()).AddTo(this);
        TouchManager.Instance.ObserveOnDrag().Subscribe(PlayerTank.Move).AddTo(this);
        PlayerTank.Departure();
        StartButton.SetActive(false);
        StartCoroutine(StageStart());
    }

    private IEnumerator StageStart()
    {
        if (GameLevel.Value == 0)
        {
            currentScore.Value = 0;
            Random.InitState(LevelDesign.RandomSeed);
        }
        spawnedEnemy.Clear();

        Debug.Log("Begin Stage Opening!!");
        yield return new WaitForSeconds(2);
        Debug.Log("Game Start!!");

        EnemySpawners.ToList().ForEach(x => { x.Initialize(); x.IsAutoSpawn = true; });
        BonusSpawner.IsAutoSpawn = false;
        RemainingGameTime = LevelDesign.GameTime;
    }

    private IEnumerator StageEnd()
    {
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);

        if (currentScore.Value > highScore.Value)
        {
            highScore.Value = currentScore.Value;
            PlayerPrefs.SetInt(HighScoreKey, highScore.Value);
        }

        Debug.Log("Begin Stage Ending");
        yield return new WaitForEndOfFrame();
        Debug.Log("End Stage Ending");

        if (++GameLevel.Value > LevelDesign.LevelMax)
        {
            yield break;
        }

        StartCoroutine(StageStart());
    }

    private void OnPlayerDead()
    {

    }

    public void OnEnemySpawn(VehicleController enemy)
    {
        spawnedEnemy.Add(enemy);
    }

    public void OnEnemyDead(VehicleController enemy)
    {
        if (enemy.Type != VehicleController.VehicleType.Bonus)
        {
            if (--remainingDefeatBonus <= 0)
            {
                BonusSpawner.Spawn(VehicleController.VehicleType.Bonus);
                remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
            }
        }

        var scorePoint = LevelDesign.DefeatScorePoint;
        switch (enemy.Type)
        {
            case VehicleController.VehicleType.Strong: scorePoint *= 3; break;
            case VehicleController.VehicleType.Bonus: scorePoint *= 10; break;
        }
        currentScore.Value += scorePoint;

        spawnedEnemy.Remove(enemy);
    }

    private void OnObjectEnterWiper(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            spawnedEnemy.Remove(other.GetComponent<EnemyController>());
        }
        Destroy(other.gameObject);
    }
}
