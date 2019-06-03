using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        var tank = FindObjectOfType<TankController>();
        TouchManager.Instance.ObserveOnTap().Subscribe(_ => tank.Fire());
        TouchManager.Instance.ObserveOnDrag().Subscribe(tank.Move);
    }
}
