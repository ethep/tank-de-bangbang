using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearWindow : MonoBehaviour
{
    private const string ScoreTextFormat = "Score <#00FF00>{0}<#000000>\nHigh <#00FF00>{1}<#000000>";

    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI ScoreText;
    public GameObject UpdateRecordsLabel;

    private bool isClicked = false;

    public IEnumerator Show(int score, int highScore)
    {
        isClicked = false;
        this.gameObject.SetActive(true);

        LevelText.text = GameManager.Instance.GameLevel.Value == LevelDesign.LevelMax ?
            "Clear !!" : "Failed ...";

        ScoreText.text = string.Format(ScoreTextFormat, score, highScore);
        UpdateRecordsLabel.SetActive(score > highScore);
        while (!isClicked)
        {
            yield return null;
        }
    }

    public void OnClick()
    {
        isClicked = true;
        this.gameObject.SetActive(false);
    }
}
