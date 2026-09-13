using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    [SerializeField] TextMeshProUGUI number;
    [SerializeField] GameObject uiparent;
    [SerializeField] TextMeshProUGUI currentScoreText;
    [Tooltip("显示本关卡历史最高分的文本，可留空")]
    [SerializeField] TextMeshProUGUI highScoreText;
    [Tooltip("刷新最高分时显示的特效/标签，可留空")]
    [SerializeField] GameObject newRecordTag;
    [SerializeField] Image starImage;

    private int threeStar = 30;
    private int currentScore = 0;
    private void Awake()
    {
        Instance = this;
        

    }
    void Start()
    {
        hide();
        GameManager.Instance.onchangstate += GameManager_onchangstate;
    }

    private void GameManager_onchangstate(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOverState())
        { 
           currentScore=OrderManager.Instance.GetCurrentScore();
           starImage.fillAmount= Mathf.Clamp01((float)currentScore/threeStar);
            Debug.Log(starImage.fillAmount);
           number.text=("成功上菜数："+OrderManager.Instance.GetsuccessDeliverCount().ToString());
           currentScoreText.text = ("本次得分：" +currentScore );
           refreshHighScore();
           show();
        }
    }
    public int GetThreeScore()
    { return threeStar; }
    /// <summary>显示本关卡的历史最高分，并标记本局是否刷新了纪录。</summary>
    private void refreshHighScore()
    {
        bool isNewRecord = GameManager.Instance.LastRunIsNewRecord;
        if (highScoreText != null)
        {
            highScoreText.text = "最高分：" + GameManager.Instance.GetLevelHighScore() + (isNewRecord ? "（新纪录！）" : "");
        }
        if (newRecordTag != null)
        {
            newRecordTag.SetActive(isNewRecord);
        }
    }
    private void show()
    { 
      uiparent.SetActive(true);
    }
    private void hide()
    {
        uiparent.SetActive(false);
    }
    private void onClickBackButton()
    { 
      
    }
    private void onClickNextButton()
    {

    }
}
