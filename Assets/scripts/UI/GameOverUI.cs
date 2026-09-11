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
           number.text=("成功上菜数"+OrderManager.Instance.GetsuccessDeliverCount().ToString());
           currentScoreText.text = ("本次得分：" +currentScore );
           show();
        }
    }
    public int GetThreeScore()
    { return threeStar; }
    private void show()
    { 
      uiparent.SetActive(true);
    }
    private void hide()
    {
        uiparent.SetActive(false);
    }
}
