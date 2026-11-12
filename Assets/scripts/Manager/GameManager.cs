using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{


    [SerializeField]private  Transform playerPrefab;
    public static GameManager Instance { get; private set; }
    public enum State
    {
        waitingtostart,
        countdowntostart,
        gameplaying,
        gameover
    }
    private NetworkVariable<State> state = new NetworkVariable<State>(State.waitingtostart);
    private NetworkVariable<float> countdownstarttime = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gameplayingtimer = new NetworkVariable<float>(120f);
    private NetworkVariable<bool> playerGamePaused = new NetworkVariable<bool>(false);
    private float gameplayingtimerTotal;

    public event EventHandler onChangState;
    public event EventHandler onLocalGamePaused;
    public event EventHandler onLocalGameUnPaused;
    public event EventHandler onLocalPlayerReadyChanged;
    public event EventHandler onMultiplayerPaused;
    public event EventHandler onMultiplayerUnPaused;

    public bool isLocalPlayerPaused { get; private set; }
    /// <summary>本局是否刷新了当前关卡的历史最高分（结算面板用）。</summary>
    public bool LastRunIsNewRecord { get; private set; }
    public bool isLocalPlayerReady { get; private set; }
    private bool  autoTestGamePausedState = false;    //客户端断联自动检测是否有其他玩家暂停游戏，避免断联后继续游戏导致游戏状态不同步

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDirctionary;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 GameManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 上一局 gameover 把 timeScale 冻在 0，换场景后没人解开，新场景会整个卡死；进场景时强制复位
        Time.timeScale = 1f;
        isLocalPlayerPaused = false;
        gameplayingtimerTotal = gameplayingtimer.Value;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDirctionary = new Dictionary<ulong, bool>();

    }
    private void Start()
    {
        //出生补偿：玩家生成时状态可能还没变化，OnStateChanged 不会触发，所以生成瞬间按当前状态补一次
        player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        gameinput.Instance.OnPause += GameInput_OnPause;
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        ApplyPlayerControl();
    }

    //取消订阅事件，避免内存泄漏
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (gameinput.Instance != null)
        {
            gameinput.Instance.OnPause -= GameInput_OnPause;
        }
        // 必须退订同一个命名方法：之前 += 和 -= 各写了一个匿名 lambda，
        // 两个委托实例不相等，等于从来没退订过
        player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += OnStateChanged;
        playerGamePaused.OnValueChanged += OnPlayerGamePausedChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventComplated;
        }
    }

    private void NetworkManager_OnLoadEventComplated(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
           
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId,true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        autoTestGamePausedState = true;  
    }

    public override void OnNetworkDespawn()
    {
        state.OnValueChanged -= OnStateChanged;
        playerGamePaused.OnValueChanged -= OnPlayerGamePausedChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= NetworkManager_OnLoadEventComplated;
    }

    private void OnPlayerGamePausedChanged(bool previousValue, bool newValue)
    {
        if (playerGamePaused.Value)
        {
            Time.timeScale = 0;
            onMultiplayerPaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1;
            onMultiplayerUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnStateChanged(State previousValue, State newValue)
    {
        // 结算时冻结世界：放在这里所有端各执行一次。
        // （原来写在服务器 Update 里，客户端冻不住，警告动画就会一直循环）
        if (newValue == State.gameover)
        {
            Time.timeScale = 0;
        }
        onChangState?.Invoke(this, EventArgs.Empty);
        ApplyPlayerControl(); //每端自己处理玩家
    }

    /// <summary>
    /// 设置本地玩家的准备状态存入字典，再检查存储的所有玩家是否都准备好了，如果都准备好了就开始倒计时
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyChangedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                return;
            }
        }
        TruntoCountDownToStart();
    }


    private void GameInput_OnPause(object sender, EventArgs e)
    {
        ToggleGame();
    }

    //只有在游戏中的状态下才允许玩家控制角色移动，避免在暂停或结算界面时误操作
    private void ApplyPlayerControl()
    {
        if (player.LocalInstance == null)    //如果本地玩家还没有生成出来，直接返回，等回补
            return;
        player.LocalInstance.enabled = state.Value == State.gameplaying;
    }

    void Update()
    {
        // 不再按时间自动推进；固定检测物理 E 键（不受 Interact 改键影响）
        // 暂停时不响应 E，避免暂停界面打开时误跳过教程
        if (!isLocalPlayerPaused && Input.GetKeyDown(KeyCode.E) && state.Value == State.waitingtostart)
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
                    gameplayingtimer.Value -= Time.deltaTime;
                    if (gameplayingtimer.Value <= 0)
                    {
                        TrunTogameover();
                    }
                }
                break;
            // gameover 不再在这里设 timeScale：服务器每帧设、客户端设不到，
            // 已挪到 OnStateChanged 里两端各设一次
            default:
                break;




        }

    }

    public void LateUpdate() //延迟进行玩家暂停状态的判断
    {
        if (autoTestGamePausedState)
        {
            autoTestGamePausedState = false;
            CheckPlayerPaused();
        }
    }
    private void TruntoCountDownToStart()
    {
        state.Value = State.countdowntostart;

    }
    private void TrunTogameplaying()
    {
        state.Value = State.gameplaying;
    }
    private void TrunTogameover()
    {
        // 必须先记分再改状态：state.Value 赋值会立刻触发 onChangState，
        // GameOverUI 在回调里读 LastRunIsNewRecord，顺序反了结算面板显示的是上一局的结果
        RecordScore();
        state.Value = State.gameover;
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


    public float Getcountdowntimer()
    {
        return countdownstarttime.Value;
    }

    /// <summary>设置面板是否打开（打开期间应暂停计时）。</summary>
    private bool IsSettingsOpen()
    {
        return SettingsUI.instance != null && SettingsUI.instance.IsOpen;
    }

    public bool CheckGameState(State stateToCheck)
    {
        return state.Value == stateToCheck;
    }
    public void ToggleGame()
    {
        isLocalPlayerPaused = !isLocalPlayerPaused;
        if (isLocalPlayerPaused)
        {
            onLocalGamePaused?.Invoke(this, EventArgs.Empty);
            SetLocalPlayerPausedServerRpc();
        }
        else
        {
            onLocalGameUnPaused?.Invoke(this, EventArgs.Empty);
            SetLocalPlayerUnPausedServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetLocalPlayerPausedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDirctionary[serverRpcParams.Receive.SenderClientId] = true;
        CheckPlayerPaused();
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetLocalPlayerUnPausedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDirctionary[serverRpcParams.Receive.SenderClientId] = false;
        CheckPlayerPaused();
    }
    private void CheckPlayerPaused()
    {
        //服务器权威 只有服务器可以修改 playerGamePaused 的值，客户端只读
        if (!IsServer)
            return;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDirctionary.ContainsKey(clientID) && playerPausedDirctionary[clientID])
            {
                playerGamePaused.Value = true;
                return;
            }
        }
        playerGamePaused.Value = false;
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
