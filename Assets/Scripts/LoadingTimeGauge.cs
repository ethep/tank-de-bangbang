using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Image))]
public class LoadingTimeGauge : MonoBehaviour
{
    public ReactiveProperty<float> Value = new ReactiveProperty<float>(1f);
    public Image GaugeImage;

    private Transform target;
    private Vector3 screenRate;

    private void Awake()
    {
        Value.Subscribe(x =>
        {
            GaugeImage.enabled = x < 1f;
            GaugeImage.fillAmount = x;
        }).AddTo(this);

        var widthRate = (float)Screen.width / (float)Camera.main.targetTexture.width;
        var heightRate = (float)Screen.height / (float)Camera.main.targetTexture.height;
        screenRate = new Vector3(widthRate, heightRate);
    }

    public void SetTarget(Transform tank)
    {
        target = tank;
        target.ObserveEveryValueChanged(x => x.transform.position)
            .Subscribe(x => (transform as RectTransform).position = RectTransformUtility.WorldToScreenPoint(Camera.main, x) * screenRate)
            .AddTo(target);
    }
}
