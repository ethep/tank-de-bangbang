using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class ColliderEventObserver : MonoBehaviour
{
    #region UniRx
    private Subject<Collider> onTriggerEnter = new Subject<Collider>();
    public IObservable<Collider> ObserveOnTriggerEnter()
    {
        return onTriggerEnter;
    }
    private Subject<Collider> onTriggerExit = new Subject<Collider>();
    public IObservable<Collider> ObserveOnTriggerExit()
    {
        return onTriggerExit;
    }
    private Subject<Collider> onTriggerStay = new Subject<Collider>();
    public IObservable<Collider> ObserveOnTriggerStay()
    {
        return onTriggerStay;
    }
    private Subject<Collision> onCollisionEnter = new Subject<Collision>();
    public IObservable<Collision> ObserveOnCollisionEnter()
    {
        return onCollisionEnter;
    }
    private Subject<Collision> onCollisionExit = new Subject<Collision>();
    public IObservable<Collision> ObserveCollisionExit()
    {
        return onCollisionExit;
    }
    private Subject<Collision> onCollisionStay = new Subject<Collision>();
    public IObservable<Collision> ObserveOnCollisionStay()
    {
        return onCollisionStay;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.OnNext(other);
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.OnNext(other);
    }

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.OnNext(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        onCollisionEnter.OnNext(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        onCollisionExit.OnNext(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        onCollisionStay.OnNext(collision);
    }
}
