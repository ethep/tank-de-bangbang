using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TankController
{
    public LoadingTimeGauge LoadingGauge;

    private new void Reset()
    {
        base.Reset();

        Type = VehicleType.Player;
    }

    private void Start()
    {
        SetParam();
        LoadingGauge?.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (LoadingGauge != null)
        {
            LoadingGauge.Value.Value = (Time.time - lastFireTime) / FireRate;
        }
    }

    public override void Move(Vector3 vec)
    {
        base.Move(vec);
    }

    public void SetParam()
    {
        MoveSpeed = LevelDesign.Player.TankSpeed();
        FireRate = LevelDesign.Player.FireRate();
        ShellSpeed = LevelDesign.Player.ShellSpeed();
    }

    public void Departure()
    {
        Rigidbody.isKinematic = false;
        lastFireTime = Time.time - FireRate;
        LoadingGauge.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        // Idleのアニメーションで動いてしまうので、強制的に戻す
        Turret.transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x - 90, // 元のモデルが回転してしまってる
            transform.eulerAngles.y - 90,
            transform.eulerAngles.z);
    }
}
