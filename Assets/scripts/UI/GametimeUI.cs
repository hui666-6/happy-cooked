using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GametimeUI : MonoBehaviour
{
    [SerializeField] private GameObject uiparent;
    [SerializeField] private Image countdown;
    [SerializeField] private TextMeshProUGUI time;
    
    public int currentTime=0;
        
        
    private void Start()
    {
        GameManager.Instance.onchangstate += GameManager_onchangstate;
        hide();
       
    }
    private void Update()
    {
        if (GameManager.Instance.IsGamePlayingState())
        {
           
            countdown.fillAmount=GameManager.Instance.GetGamePlayingTimeNormal();
            time.text= Mathf.CeilToInt( GameManager.Instance.GetGamePlayingTime()).ToString();
        }
    }

    private void GameManager_onchangstate(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGamePlayingState())
        {
            show();
        
        }
    }
    private void hide()
    { 
     uiparent.SetActive(false);
    }
    private void show()
    { uiparent.SetActive(true); }

}
