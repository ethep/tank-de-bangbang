using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class EnemyController : TankController
{
    public enum EnemyType
    {
        Normal,
        Strong,
        Bonus,
    };

    public EnemyType Type = EnemyType.Normal;

    private void Start()
    {
        Observable.Interval(TimeSpan.FromSeconds(FireRate))
            .Subscribe(_ => Fire())
            .AddTo(this);
    }

    private void Update()
    {
        Move(this.transform.forward);
    }

    private void LateUpdate()
    {
        // Idleのアニメーションで動いてしまうので、強制的に戻す
        var rot = 180 - transform.rotation.eulerAngles.y;
        Turret.transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x - 90, // 元のモデルが回転してしまってる
            transform.eulerAngles.y + rot,
            transform.eulerAngles.z);
    }
}
