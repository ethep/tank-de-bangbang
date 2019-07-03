using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

public class BonusPlaneController : VehicleController
{
    public AudioClip TankDead;

    private new void Reset()
    {
        base.Reset();

        TankDead = AssetDatabase.FindAssets("t:audioclip tankDead").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();

        Type = VehicleType.Bonus;
        MoveSpeed = LevelDesign.Enemy.BonusSpeed;
    }

    protected override void Damage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        HitPoint = 0;

        OnDead.OnNext(Unit.Default);

        Animator.SetBool("Dead2", true);

        SoundSource.loop = false;
        SoundSource.pitch = 1;
        SoundSource.clip = TankDead;
        SoundSource.Play();

        Collider.enabled = false;

        StartCoroutine(DeadWipe());
        IEnumerator DeadWipe()
        {
            yield return new WaitForSeconds(2f);
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        Move(this.transform.forward);
    }
}
