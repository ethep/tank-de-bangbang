using Lean.Touch;
using System;
using UniRx;
using UnityEditor;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    public Camera Camera;
    public LeanTouch leanTouch;
    public bool EnableTouch;

    #region UniRx

    private Subject<Vector3> onTap;

    public IObservable<Vector3> ObserveOnTap()
    {
        return onTap ?? (onTap = new Subject<Vector3>());
    }

    private Subject<Vector3> onDrag;

    public IObservable<Vector3> ObserveOnDrag()
    {
        return onDrag ?? (onDrag = new Subject<Vector3>());
    }

    private Subject<Vector3> onSwipe;

    public IObservable<Vector3> ObserveOnSwipe()
    {
        return onSwipe ?? (onSwipe = new Subject<Vector3>());
    }

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    protected virtual void OnEnable()
    {
        LeanTouch.OnFingerTap += FingerTap;
        LeanTouch.OnFingerSet += FingerSet;
        LeanTouch.OnFingerUp += FingerUp;
        LeanTouch.OnFingerSwipe += FingerSwipe;
    }

    protected virtual void OnDisable()
    {
        LeanTouch.OnFingerTap -= FingerTap;
        LeanTouch.OnFingerSet -= FingerSet;
        LeanTouch.OnFingerUp -= FingerUp;
        LeanTouch.OnFingerSwipe -= FingerSwipe;
    }

    private void FingerTap(LeanFinger finger)
    {
        if (!EnableTouch)
        {
            return;
        }

        if (finger.StartedOverGui == true)
        {
            return;
        }

        onTap?.OnNext(finger.StartScreenPosition);
    }

    private void FingerSet(LeanFinger finger)
    {
        if (!EnableTouch)
        {
            return;
        }

        if (finger.StartedOverGui == true)
        {
            return;
        }

        var distance = finger.LastScreenPosition - finger.StartScreenPosition;
        if (distance.magnitude <= 0)
        {
            return;
        }

        onDrag?.OnNext(distance);
    }

    private void FingerUp(LeanFinger finger)
    {
        if (!EnableTouch)
        {
            return;
        }
    }

    private void FingerSwipe(LeanFinger finger)
    {
        if (!EnableTouch)
        {
            return;
        }

        if (finger.StartedOverGui == true)
        {
            return;
        }

        onSwipe?.OnNext(finger.SwipeScreenDelta);
    }
}