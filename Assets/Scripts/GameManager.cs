using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Game Object
    public PlayerController PlayerTank;
    public ColliderEventObserver[] Wiper;
    public EnemySpawner[] EnemySpawners;
    public EnemySpawner BonusSpawner { get { return EnemySpawners[3]; } }

    // UI
    public GameObject StartButton;

    // Game Parameter
    public int GameLevel = 0;
    public int Score = 0;
    public float RemainingGameTime = 0;
    public bool InGame { get { return RemainingGameTime > 0; } }

    private int remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
    private List<VehicleController> spawnedEnemy = new List<VehicleController>();

    private void Awake()
    {
        Instance = this;

        Wiper.ToList().ForEach(x => x.ObserveOnTriggerEnter().Subscribe(OnObjectEnterWiper).AddTo(this));
        PlayerTank.ObserveOnDead().Subscribe(_ => OnPlayerDead());
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);
    }

    private void Update()
    {
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
        if (GameLevel == 0)
        {
            Score = 0;
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

        Debug.Log("Begin Stage Ending");
        yield return new WaitForEndOfFrame();
        Debug.Log("End Stage Ending");

        if (++GameLevel > LevelDesign.LevelMax)
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
        if (--remainingDefeatBonus <= 0)
        {
            BonusSpawner.Spawn(VehicleController.VehicleType.Bonus);
            remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
        }

        var scorePoint = LevelDesign.DefeatScorePoint;
        switch (enemy.Type)
        {
            case VehicleController.VehicleType.Strong: scorePoint *= 3; break;
            case VehicleController.VehicleType.Bonus: scorePoint *= 10; break;
        }
        Score += scorePoint;

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
