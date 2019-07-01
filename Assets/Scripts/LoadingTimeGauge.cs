using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Image))]
public class LoadingTimeGauge : MonoBehaviour
{
    public Transform target;
    public ReactiveProperty<float> Value = new ReactiveProperty<float>(1f);

    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        Value.Subscribe(x =>
        {
            image.enabled = x < 1f;
            image.fillAmount = x;
        }).AddTo(this);

        var widthRate = (float)Screen.width / (float)Camera.main.targetTexture.width;
        var heightRate = (float)Screen.height / (float)Camera.main.targetTexture.height;
        var screenRate = new Vector3(widthRate, heightRate);

        target.ObserveEveryValueChanged(x => x.transform.position)
            .Subscribe(x => rectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, x) * screenRate)
            .AddTo(target);
    }
}
