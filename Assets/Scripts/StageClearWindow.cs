using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearWindow : MonoBehaviour
{
    private const string ScoreTextFormat = "Score <#00FF00>{0}<#000000>\nTotal <#00FF00>{1}<#000000>";

    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI ScoreText;
    public GameObject UpdateRecordsLabel;

    public void Show(int stageScore, int totalScore, bool isHighscoreUpdate)
    {
        this.gameObject.SetActive(true);

        LevelText.text = GameManager.Instance.GameLevel.Value == LevelDesign.LevelMax ?
            "Final Result" : string.Format("Lv.{0} Result", GameManager.Instance.GameLevel.Value + 1);
        ScoreText.text = string.Format(ScoreTextFormat, stageScore, totalScore);
        UpdateRecordsLabel.SetActive(isHighscoreUpdate);
    }
}
