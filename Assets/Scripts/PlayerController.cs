using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TankController
{
    private void LateUpdate()
    {
        // Idleのアニメーションで動いてしまうので、強制的に戻す
        Turret.transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x - 90, // 元のモデルが回転してしまってる
            transform.eulerAngles.y - 90,
            transform.eulerAngles.z);
    }
}
