using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int GameLevel = 0;
    public PlayerController PlayerTank;
    public ColliderEventObserver[] Wiper;
    public EnemySpawner[] EnemySpawners;
    public EnemySpawner BonusSpawners;

    public int Score = 0;
    public float RemainingGameTime = 0;
    public bool InGame { get { return RemainingGameTime > 0; } }

    private int remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
    private List<EnemyController> spawnedEnemy = new List<EnemyController>();

    private void Awake()
    {
        Instance = this;

        // ワイパーの準備
        Wiper.ToList().ForEach(x => x.ObserveOnTriggerEnter().Subscribe(OnObjectEnterWiper).AddTo(this));

        // プレイヤーの準備

        LevelDesign.Player.FireRateLevel = 0;
        LevelDesign.Player.ShellSpeedLevel = 0;
        LevelDesign.Player.TankSpeedLevel = 0;


        TouchManager.Instance.ObserveOnTap().Subscribe(_ => PlayerTank.Fire());
        TouchManager.Instance.ObserveOnDrag().Subscribe(PlayerTank.Move);
        PlayerTank.ObserveOnDead().Subscribe(_ => OnPlayerDead());
        PlayerTank.FireRate = LevelDesign.Player.FireRate();
        PlayerTank.TankSpeed = LevelDesign.Player.TankSpeed();
        PlayerTank.ShellSpeed = LevelDesign.Player.ShellSpeed();

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

    public void OnEnemySpawn(EnemyController enemy)
    {
        spawnedEnemy.Add(enemy);
    }

    public void OnEnemyDead(EnemyController enemy)
    {
        if (--remainingDefeatBonus <= 0)
        {
            BonusSpawners.Spawn(EnemyController.EnemyType.Bonus);
            remainingDefeatBonus = LevelDesign.Enemy.BonusRequiredDefeat;
        }

        var scorePoint = LevelDesign.DefeatScorePoint;
        switch (enemy.Type)
        {
            case EnemyController.EnemyType.Normal: break;
            case EnemyController.EnemyType.Strong: scorePoint *= 3; break;
            case EnemyController.EnemyType.Bonus: scorePoint *= 10; break;
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
