using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    private enum State
    {
        waitingtostart,
        countdowntostart,
        gameplaying,
        gameover
    }
    private NetworkVariable<State> state = new NetworkVariable<State>(State.waitingtostart);
    private NetworkVariable<float> countdownstarttime = new NetworkVariable<float>(3f);
    private NetworkVariable < float> gameplayingtimer = new NetworkVariable<float>(120f);
    private float gameplayingtimerTotal;

    [SerializeField] private player player;
    public event EventHandler onChangState;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;
    public event EventHandler onLocalPlayerReadyChanged;

    private bool isgamePause = false;
    /// <summary>本局是否刷新了当前关卡的历史最高分（结算面板用）。</summary>
    public bool LastRunIsNewRecord { get; private set; }
    public bool isLocalPlayerReady { get; private set; }

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 GameManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameplayingtimerTotal = gameplayingtimer.Value;
        playerReadyDictionary = new Dictionary<ulong, bool>();

    }
    private void Start()
    {
        TrunTowaitingtostart();
        gameinput.Instance.OnPause += GameInput_OnPause;
    }

    public  override void  OnDestroy()
    {
        if (gameinput.Instance != null)
        {
            gameinput.Instance.OnPause -= GameInput_OnPause;
        }
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += OnStateChanged;
    }

    private void OnStateChanged(State previousValue, State newValue)
    {
        onChangState?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyChangedServerRpc(ServerRpcParams serverRpcParams=default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                return;
            }       
             TruntoCountDownToStart();
        }
    }


    private void GameInput_OnPause(object sender, EventArgs e)
    {
        ToggleGame();
    }

   
    void Update()
    {
        // 不再按时间自动推进；固定检测物理 E 键（不受 Interact 改键影响）
        // 暂停时不响应 E，避免暂停界面打开时误跳过教程
        if (!isgamePause && Input.GetKeyDown(KeyCode.E)&&state.Value==State.waitingtostart)
        {
            isLocalPlayerReady = true;
            onLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyChangedServerRpc();
        }

        if (!IsServer)
            return;
        switch (state.Value)
        {
                     
            case State.countdowntostart:
                countdownstarttime.Value -= Time.deltaTime;

                if (countdownstarttime.Value <= 0)
                {
                    TrunTogameplaying();

                }
                break;
            case State.gameplaying:
                // 设置面板打开时停止计时，避免面板显示期间倒计时仍在流逝
                if (!IsSettingsOpen())
                {
                    // gameplayingtimer -= Time.deltaTime;
                    if (gameplayingtimer .Value<= 0)
                    {
                        TrunTogameover();
                    }
                }
                break;
            case State.gameover:
                break;
            default:
                break;




        }

    }

    private void TrunTowaitingtostart()
    {
        state.Value = State.waitingtostart;
        DisablePlayer();
        onChangState?.Invoke(this, EventArgs.Empty);
    }

    private void TruntoCountDownToStart()
    {
        state.Value = State.countdowntostart;
        DisablePlayer();
        onChangState?.Invoke(this, EventArgs.Empty);
    }
    private void TrunTogameplaying()
    {
        state.Value = State.gameplaying;
        EnablePlayer();
        onChangState?.Invoke(this, EventArgs.Empty);

    }
    private void TrunTogameover()
    {
        state.Value = State.gameover;
        DisablePlayer();
        RecordScore();
        onChangState?.Invoke(this, EventArgs.Empty);

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
    private void DisablePlayer()
    {
        // player.enabled = false;

    }
    private void EnablePlayer()
    {
        //  player.enabled = true;

    }
    public bool IsCountDownToStart()
    {
        return state.Value == State.countdowntostart;

    }
    public float Getcountdowntimer()
    {
        return countdownstarttime.Value;
    }
    public bool IsGamePlayingState()
    {
        return state.Value == State.gameplaying;
    }
    /// <summary>设置面板是否打开（打开期间应暂停计时）。</summary>
    private bool IsSettingsOpen()
    {
        return SettingsUI.instance != null && SettingsUI.instance.IsOpen;
    }
    public bool IsGameOverState()
    {
        return state.Value == State.gameover;
    }
    public bool IsGameWaitingToStart()
    {

        return state.Value == State.waitingtostart;
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
            Time.timeScale = 1;
            OnGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }
    public float GetGamePlayingTime()
    {
        return gameplayingtimer.Value;
    }
    public float GetGamePlayingTimeNormal()
    {
        return gameplayingtimer.Value / gameplayingtimerTotal;
    }
}
