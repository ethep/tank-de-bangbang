using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class VehicleController : MonoBehaviour
{
    public enum VehicleType
    {
        Player,
        Enemy,
        Strong,
        Bonus,
    };

    // Component
    public Animator Animator;
    public AudioSource SoundSource;
    public Collider Collider;
    public Rigidbody Rigidbody;

    // Parameter
    public VehicleType Type = VehicleType.Enemy;
    public float MoveSpeed;
    public int HitPoint = 1;
    public bool IsDead { get { return HitPoint <= 0; } }
    public bool Pause { get; set; } = false;

    #region UniRx
    protected Subject<Unit> OnDead = new Subject<Unit>();
    public IObservable<Unit> ObserveOnDead()
    {
        return OnDead;
    }
    #endregion

    protected void Reset()
    {
        Animator = GetComponentInChildren<Animator>();
        SoundSource = GetComponent<AudioSource>();
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
    }

    public virtual void Move(Vector3 vec)
    {
        if (IsDead || Pause)
        {
            return;
        }

        Vector3 movement = transform.forward * vec.normalized.x * MoveSpeed * Time.deltaTime;
        Rigidbody.velocity = Rigidbody.transform.forward * movement.x;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsDead || Pause)
        {
            return;
        }

        if (!other.CompareTag("Shell"))
        {
            return;
        }

        var shell = other.GetComponent<Shell>();
        if (CompareTag(shell.ParentTag))
        {
            return;
        }

        Damage(shell.Damage);
    }

    protected abstract void Damage(int damage);
}
