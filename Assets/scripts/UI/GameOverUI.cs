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

    private int CurrentLevelIndex => LevelManager.Instance.GetLevelDefinition().LevelKey != null ? LevelManager.Instance.GetLevelDefinition().LevelKey.GetHashCode() : 0;
    private int threeStar => LevelManager.Instance.GetLevelDefinition().star3Score;
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
    public void onClickBackButton()
    {
        Loader.LoadScene("LevelChooseScence");
    }
    public void onClickNextButton()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("GameOverUI：找不到 LevelManager，无法加载下一关", this);
            return;
        }

        // 从“当前关卡”的配置里读它自己的下一关，这样复用的结算面板每关都会跳到正确的场景
        LevelDefinitionSO current = LevelManager.Instance.GetLevelDefinition();
        if (current == null)
        {
            Debug.LogError("GameOverUI：当前关卡未配置 LevelDefinition，无法加载下一关", this);
            return;
        }

        LevelDefinitionSO next = current.nextLevel;
        if (next == null || string.IsNullOrWhiteSpace(next.sceneName))
        {
            Debug.LogWarning("GameOverUI：当前关卡没有配置下一关（可能已是最后一关）", this);
            return;
        }

        Loader.LoadScene(next.sceneName);
    }

   
  
}
