using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabList = new List<GameObject>();
    public float SpawnInterval = 10.0f;
    public float RemainingTime;

    private void Reset()
    {
        enemyPrefabList.Clear();
        enemyPrefabList.AddRange(
            Resources.LoadAll<EnemyController>("Prefabs/Enemy")
                .ToList()
                .ConvertAll(x => x.gameObject));
    }

    private void Start()
    {
        RemainingTime = Random.Range(SpawnInterval * 0.5f, SpawnInterval);
    }

    private void Update()
    {
        if ((RemainingTime -= Time.deltaTime) > 0)
        {
            return;
        }

        Spawn();
        RemainingTime = Random.Range(SpawnInterval * 0.5f, SpawnInterval * 1.5f);
    }

    public void Spawn()
    {
        var prefab = enemyPrefabList[Random.Range(0, enemyPrefabList.Count)];
        var newEnemy = Instantiate(prefab, this.transform).GetComponent<EnemyController>();
        newEnemy.transform.position = this.transform.position;
        newEnemy.transform.localRotation = Quaternion.identity;

        newEnemy.ObserveOnDead()
            .Subscribe(_ => GameManager.Instance.EnemyDead(newEnemy))
            .AddTo(newEnemy);
    }
}
