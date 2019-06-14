using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        var tank = FindObjectOfType<TankController>();
        TouchManager.Instance.ObserveOnTap().Subscribe(_ => tank.Fire());
        TouchManager.Instance.ObserveOnDrag().Subscribe(tank.Move);
        tank.ObserveOnDead().Subscribe(_ => PlayerDead());
    }

    private void PlayerDead()
    {

    }

    public void EnemyDead(EnemyController enemy)
    {

    }
}
