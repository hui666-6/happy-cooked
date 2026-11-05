using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI number;
    private Animator animator;
    private const string IS_SHAKE = "IsShake";
    private int prenumber = -1;
    private void Start()
    {   
        animator = GetComponent<Animator>(); 
        GameManager.Instance.onChangState += GameManager_onchangstate;            
    }
    private void Update()
    {
        
        if (GameManager.Instance.CheckGameState(GameManager.State.countdowntostart))
        {
            int NowNumber = Mathf.CeilToInt(GameManager.Instance.Getcountdowntimer());
            number.text=NowNumber.ToString();
            if(prenumber!=NowNumber)
            {
                prenumber = NowNumber;
                animator.SetTrigger(IS_SHAKE);
                SoundManager.instance.countdownsound();

            }
        }
    }

    private void GameManager_onchangstate(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.CheckGameState(GameManager.State.countdowntostart))
        {
            number.gameObject.SetActive(true);
        }
        else
        {
            number.gameObject.SetActive(false);
        }
    }
}
  
