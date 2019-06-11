using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : TankController
{
    private void Update()
    {
        Move(this.transform.forward);
    }
}
