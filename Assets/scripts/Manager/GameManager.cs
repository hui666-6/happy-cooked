using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private enum State
    { 
        waitingtostart,
        countdowntostart,
        gameplaying,
        gameover    
    }
    private State state;
    private float countdownstarttime = 3;
    private float gameplayingtimer = 60;
    private float gameplayingtimerTotal;
    [SerializeField] private player player;
    public event EventHandler onchangstate;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;
    private bool isgamePause=false;
    /// <summary>本局是否刷新了当前关卡的历史最高分（结算面板用）。</summary>
    public bool LastRunIsNewRecord { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 GameManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameplayingtimerTotal = gameplayingtimer;
       
    }
    private void Start()
    {
        TrunTowaitingtostart();
        gameinput.Instance.OnPause += GameInput_OnPause;
    }

    private void OnDestroy()
    {
        if (gameinput.Instance != null)
        {
            gameinput.Instance.OnPause -= GameInput_OnPause;
        }
    }

    private void GameInput_OnPause(object sender, EventArgs e)
    {
        ToggleGame();
    }

    void  Update()
    {
        switch (state)
        { 
            case State.waitingtostart:
                // 不再按时间自动推进；固定检测物理 E 键（不受 Interact 改键影响）
                // 暂停时不响应 E，避免暂停界面打开时误跳过教程
                if (!isgamePause && Input.GetKeyDown(KeyCode.E))
                {
                    TruntoCountDownToStart();
                }
                break;
            case State.countdowntostart:
                countdownstarttime -= Time.deltaTime;
               
                if (countdownstarttime <= 0)
                {
                    TrunTogameplaying();
                
                }
                break ;
            case State.gameplaying:
                // 设置面板打开时停止计时，避免面板显示期间倒计时仍在流逝
                if (!IsSettingsOpen())
                {
                    gameplayingtimer -= Time.deltaTime;
                    if (gameplayingtimer <= 0)
                    {
                        TrunTogameover();
                    }
                }
                break ;
            case State.gameover:
                break ;
                default:
                break;
        
        
        
        
        }
        
    }

    private void TrunTowaitingtostart()
    { 
      state= State.waitingtostart;
      DisablePlayer();
      onchangstate?.Invoke(this, EventArgs.Empty);
    }

    private void TruntoCountDownToStart()
    { 
        state=State.countdowntostart;
        DisablePlayer() ;
        onchangstate?.Invoke(this, EventArgs.Empty);
    }
    private void TrunTogameplaying()
    { 
      state= State.gameplaying;
      EnablePlayer();
      onchangstate?.Invoke(this, EventArgs.Empty);

    }
    private void TrunTogameover()
    { 
     state =State.gameover;
     DisablePlayer();
     RecordScore();
     onchangstate?.Invoke(this, EventArgs.Empty);

    }

    /// <summary>结算时把本局得分提交到本地最高分存档。</summary>
    private void RecordScore()
    {
        int score = OrderManager.Instance != null ? OrderManager.Instance.GetCurrentScore() : 0;
        int stars = LevelManager.Instance != null ? LevelManager.Instance.ComputeStars(score) : 0;
        Debug.Log("GameManager：本局得分 " + score + "，星级 " + stars);
        LastRunIsNewRecord = PlayerProgress.SubmitResult(PlayerProgress.CurrentLevelKey, score, stars);
    }

    /// <summary>当前关卡的历史最高分（结算面板用）。</summary>
    public int GetLevelHighScore()
    {
        return PlayerProgress.GetCurrentLevelHighScore();
    }
    private  void DisablePlayer()
    { 
        player.enabled = false;
      
    }
    private void EnablePlayer()
    {
        player.enabled = true;
        
    }
    public bool IsCountDownToStart()
    { 
      return state==State.countdowntostart;
  
    }
    public float Getcountdowntimer()
    { 
      return countdownstarttime;
    }
    public bool IsGamePlayingState()
    {
        return state==State.gameplaying; 
    }
    /// <summary>设置面板是否打开（打开期间应暂停计时）。</summary>
    private bool IsSettingsOpen()
    {
        return SettingsUI.instance != null && SettingsUI.instance.IsOpen;
    }
    public bool IsGameOverState()
    {
        return state == State.gameover;
    }
    public bool IsGameWaitingToStart()
    { 
    
      return  state==State.waitingtostart;
    }
    public void ToggleGame()
    {
        isgamePause = !isgamePause;
        if (isgamePause)
        {
            Time.timeScale = 0;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale= 1;
            OnGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }
    public float GetGamePlayingTime()
    { 
      return gameplayingtimer;
    }
    public float GetGamePlayingTimeNormal()
    { 
     return gameplayingtimer/gameplayingtimerTotal;
    }
}
