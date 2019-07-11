using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool IsAutoSpawn = false;
    public float FirstInterval = 5;
    public float IntervalMagnification = 1.0f;
    public List<GameObject> enemyPrefabList = new List<GameObject>();

    private float remainingTime = 0f;

    private void Reset()
    {
        enemyPrefabList.Clear();
        enemyPrefabList.AddRange(
            Resources.LoadAll<EnemyController>("Prefabs/Enemy")
                .ToList()
                .ConvertAll(x => x.gameObject));
    }

    public void Initialize()
    {
        remainingTime = UnityEngine.Random.Range(FirstInterval * 0.5f, FirstInterval);
    }

    private void Update()
    {
        if (!IsAutoSpawn)
        {
            return;
        }

        if ((remainingTime -= Time.deltaTime) > 0)
        {
            return;
        }

        var spawnType = LevelDesign.Enemy.LotteryStrongSpawn() ? VehicleController.VehicleType.Strong : VehicleController.VehicleType.Enemy;
        Spawn(spawnType);

        var interval = IntervalMagnification * LevelDesign.Enemy.SpawnInterval();
        remainingTime = UnityEngine.Random.Range(interval * 0.7f, interval * 1.3f);
    }

    public void Spawn(VehicleController.VehicleType enemyType)
    {
        var prefab = enemyPrefabList[UnityEngine.Random.Range(0, enemyPrefabList.Count)];
        var newEnemy = Instantiate(prefab, this.transform).GetComponent<VehicleController>();
        newEnemy.transform.position = this.transform.position;
        newEnemy.transform.localRotation = Quaternion.identity;
        newEnemy.Type = enemyType;

        if (enemyType == VehicleController.VehicleType.Strong)
        {
            newEnemy.transform.localScale *= 1.5f;
            newEnemy.HitPoint = 2;
        }

        newEnemy.ObserveOnDead()
            .Subscribe(_ => GameManager.Instance.OnEnemyDead(newEnemy))
            .AddTo(newEnemy);

        GameManager.Instance.OnEnemySpawn(newEnemy);
    }
}
