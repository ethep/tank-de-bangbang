using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : TankController
{
    protected new void Reset()
    {
        base.Reset();
    }

    private void Update()
    {
        Move(this.transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Shell"))
        {
            return;
        }

        var shell = other.GetComponent<Shell>();
        if (shell.Parent.CompareTag(this.tag))
        {
            return;
        }

        Damage(shell.Damage);
    }
}
