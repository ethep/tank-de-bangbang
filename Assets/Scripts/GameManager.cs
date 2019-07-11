using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const string HighScoreKey = "high_score";

    // Game Object
    public Transform PlayerTankPos;
    public ColliderEventObserver[] Wiper;
    public EnemySpawner[] EnemySpawners;
    public EnemySpawner BonusSpawner { get { return EnemySpawners[3]; } }
    public Transform CloudTransform;
    public CinemachineVirtualCamera VirtualCamera;

    // UI
    public GameObject StartButton;
    public Text CurrentScoreText;
    public Text HighScoreText;
    public Text CurrentLevelText;
    public Image LevelRemainingTimeGauge;
    public LoadingTimeGauge LoadingGauge;
    public GameObject TankSelectUI;
    public PlayerController[] SelectTanks;
    public GameObject TankSelectButton;
    public SimpleAnimation LevelNoticeAnim;
    public Text LevelNoticeLabel;
    public StageClearWindow StageClearWindow;

    // Game Parameter
    public ReactiveProperty<int> GameLevel = new ReactiveProperty<int>(0);
    public float RemainingGameTime = 0;

    public PlayerController playerTank;
    private int remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
    private List<VehicleController> spawnedEnemy = new List<VehicleController>();
    private ReactiveProperty<int> currentScore = new ReactiveProperty<int>(0);
    private ReactiveProperty<int> highScore = new ReactiveProperty<int>(0);
    private bool isInStage = false;
    private int selectTankIndex = 0;

    private void Awake()
    {
        Instance = this;

        Wiper.ToList().ForEach(x => x.ObserveOnTriggerEnter().Subscribe(OnObjectEnterWiper).AddTo(this));
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);
        currentScore.Subscribe(x => CurrentScoreText.text = string.Format("Score:{0}", x)).AddTo(this);
        highScore.Subscribe(x => HighScoreText.text = string.Format("High  :{0}", x)).AddTo(this);
        GameLevel.Subscribe(x => CurrentLevelText.text = string.Format("{0} / {1}", x, LevelDesign.LevelMax)).AddTo(this);

        highScore.Value = PlayerPrefs.GetInt(HighScoreKey);
        StageClearWindow.gameObject.SetActive(false);
        GameInit();
        ResetPlayerTank();
    }

    private void Update()
    {
        LevelRemainingTimeGauge.fillAmount = RemainingGameTime / LevelDesign.GameTime;
        CloudTransform.Rotate(Vector3.up, 0.1f);
        if (!isInStage)
        {
            return;
        }

        if ((RemainingGameTime -= Time.deltaTime) <= 0)
        {
            StageEnd();
        }
    }

    private void ResetPlayerTank()
    {
        playerTank.LoadingGauge = LoadingGauge;
        LoadingGauge.SetTarget(playerTank.transform);
        playerTank.enabled = true;

        playerTank.transform.localPosition = Vector3.zero;
        playerTank.transform.localRotation = Quaternion.identity;

        VirtualCamera.Follow = playerTank.transform;
        VirtualCamera.LookAt = playerTank.transform;

        TouchManager.Instance.ObserveOnTap().Subscribe(_ => playerTank.Fire()).AddTo(playerTank);
        TouchManager.Instance.ObserveOnDrag().Subscribe(playerTank.Move).AddTo(playerTank);
        playerTank.ObserveOnDead().Subscribe(_ => StartCoroutine(GameEnd(false))).AddTo(playerTank);
    }

    private void GameInit()
    {
        spawnedEnemy.ForEach(x => Destroy(x.gameObject));
        spawnedEnemy.Clear();

        OnTapTankSelect(selectTankIndex);
        playerTank.Departure();

        StartButton.SetActive(true);
        TankSelectButton.SetActive(false);
    }

    public void GameStart()
    {
        StartButton.SetActive(false);
        TankSelectButton.SetActive(false);

        playerTank.Departure();
        StartCoroutine(StageStart());
    }

    private IEnumerator GameEnd(bool isClear)
    {
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);
        spawnedEnemy.ForEach(x => x.Pause = true);
        foreach (var x in GameObject.FindGameObjectsWithTag("Shell"))
        {
            Destroy(x);
        }

        yield return StageClearWindow.Show(currentScore.Value, highScore.Value);

        GameInit();
    }

    private IEnumerator StageStart()
    {
        if (GameLevel.Value == 0)
        {
            currentScore.Value = 0;
            Random.InitState(LevelDesign.RandomSeed);
        }

        yield return NoticeGameLevel();

        EnemySpawners.ToList().ForEach(x => { x.Initialize(); x.IsAutoSpawn = true; });
        BonusSpawner.IsAutoSpawn = false;
        RemainingGameTime = LevelDesign.GameTime;
        isInStage = true;
    }

    private void StageEnd()
    {
        isInStage = false;
        EnemySpawners.ToList().ForEach(x => x.IsAutoSpawn = false);

        if (GameLevel.Value + 1 > LevelDesign.LevelMax)
        {
            // ボーナスとして、画面内の敵を全てダメージを与える
            StartCoroutine(GameEnd(true));
            return;
        }

        GameLevel.Value++;
        StartCoroutine(StageStart());
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

        switch (enemy.Type)
        {
            case VehicleController.VehicleType.Enemy: AddScore(LevelDesign.DefeatTankPoint); break;
            case VehicleController.VehicleType.Strong: AddScore(LevelDesign.DefeatStrongPoint); break;
            case VehicleController.VehicleType.Bonus: AddScore(LevelDesign.DefeatBonusPoint); break;
        }

        spawnedEnemy.Remove(enemy);
    }

    private void AddScore(int point)
    {
        currentScore.Value += point;

        bool isHighScoreUpdate = currentScore.Value > highScore.Value;
        if (isHighScoreUpdate)
        {
            highScore.Value = currentScore.Value;
            PlayerPrefs.SetInt(HighScoreKey, highScore.Value);
        }
    }

    private void OnObjectEnterWiper(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            spawnedEnemy.Remove(other.GetComponent<EnemyController>());
        }
        Destroy(other.gameObject);
    }

    public void OnChangeTankButton()
    {
        TankSelectUI.SetActive(true);
    }

    public void OnTapTankSelect(int index)
    {
        selectTankIndex = index;
        if (playerTank != null)
        {
            Destroy(playerTank.gameObject);
        }
        playerTank = Instantiate(SelectTanks[index], PlayerTankPos);
        ResetPlayerTank();

        TankSelectUI.SetActive(false);
    }

    public IEnumerator NoticeGameLevel()
    {
        LevelNoticeLabel.text = GameLevel.Value == LevelDesign.LevelMax ?
            "Final" : string.Format("Level {0}", GameLevel.Value + 1);
        LevelNoticeAnim.Stop("Default");
        LevelNoticeAnim.Play("Default");

        yield return new WaitForSeconds(2f);
        /*
        while (LevelNoticeAnim.isPlaying)
        {
            yield return null;
        }
        */
    }

    public IEnumerator NoticeStageScore()
    {
        yield return null;
    }
}
