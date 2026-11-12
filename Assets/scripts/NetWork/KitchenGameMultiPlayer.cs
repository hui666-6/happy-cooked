using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiPlayer : NetworkBehaviour
{
    public static KitchenGameMultiPlayer Instance { get; private set; }
    public KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] public List<Color> playerSelectColorList = new List<Color>();


    public const int playerMaxCount = 4;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataListChanged;

    public NetworkList<PlayerData> playerDataList;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataList = new NetworkList<PlayerData>();
        playerDataList.OnListChanged += playerDataList_OnListChanged;
    }

    //玩家每次退出或加入都更新一下监听者的状态
    private void playerDataList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsClientPlayerID(int Index)
    {
        return Index < playerDataList.Count;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetWorkManager_ClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientExitCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_OnClientExitCallback(ulong clientid)
    {
        playerDataList.RemoveAt (GetIndexFromClient(clientid));

    }

    private void NetWorkManager_ClientConnectedCallback(ulong clientID)
    {
        PlayerData playerData = new PlayerData();
        playerData.clientID = clientID;
        playerData.colorId=GetDefaultColorId();
        playerDataList.Add(playerData);
    }

    private int GetDefaultColorId()
    {
        for (int i = 0; i < playerSelectColorList.Count; i++)
        {
           if(ColorIsAvailable(i))
                return i;
        }
        return -1;
    }
    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != Loader.scene.CharacterChooseScene.ToString())
        {
            response.Approved = false;
            response.Reason = "游戏已经开始了";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClients.Count < playerMaxCount)
        {
            response.Approved = true;
        }
    }
    /// <summary>
    /// 获取playerdata
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataList[playerIndex];
    }

    //通过clientID获取playerdata  
    public PlayerData GetPlayerDataFromClient(ulong clientid)
    {
        foreach (PlayerData playerData in playerDataList)
        {
            if (playerData.clientID == clientid)
            {
                return playerData;
            }
        }
        return default;
    }

    //playerdata是值类型 如果要改里面的值 需要通过clientID获取playerdata在列表里对应的索引取出来改列表里面的值再放回去
    public int GetIndexFromClient(ulong clientId)
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].clientID == clientId)
                return i;
        }
        return -1;
    }

    public Color GetColor(int colorID)
    {
        if (colorID < 0 || colorID >= playerSelectColorList.Count)
        {
            return Color.white;
        }
        return playerSelectColorList[colorID];
    }
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallBack;
        NetworkManager.Singleton.StartClient();

    }

    private void NetworkManager_OnClientDisconnectCallBack(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void CreateKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectHolder kitchenObjectHolder)
    {
        CreateKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectHolder.GetNetWorkObject());
    }

    [ServerRpc(RequireOwnership = false)] //标记成SeverRPC客户端调用它时，代码不会在客户端执行，而是发送消息给服务器执行  如果owner为true，就只有服务器自己能调用，客户端一按就被拒
    public void CreateKitchenObjectServerRpc(int KitchenObjectSOIndex, NetworkObjectReference kitchenObjectHolderNetWorkReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromList(KitchenObjectSOIndex);
        if (kitchenObjectSO == null)
        {
            return;
        }
        NetworkObject KitchenObjectNetWorkObject = Instantiate(kitchenObjectSO.prefab).GetComponent<NetworkObject>();
        KitchenObjectNetWorkObject.Spawn(true);//在服务器上实例化预制体，把这个对象存在的消息发给所有客户端，客户端会自动创建一份副本 参数true是destroyWithScence.场景卸载时一并销毁
        KitchenObject kitchenObject = KitchenObjectNetWorkObject.GetComponent<KitchenObject>();

        kitchenObjectHolderNetWorkReference.TryGet(out NetworkObject kitchenObjectHolderNetWorkObject);
        IKitchenObjectHolder KitchenObjectParent = kitchenObjectHolderNetWorkObject.GetComponent<IKitchenObjectHolder>();
        kitchenObject.SetKitchenobjectParent(KitchenObjectParent);//设置该对象的归属 广播该对象归谁
    }

    public KitchenObjectSO GetKitchenObjectSOFromList(int index)
    {
        return kitchenObjectListSO.kitchenObjectsSOList[index];
    }
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectsSOList.IndexOf(kitchenObjectSO);
    }
    public void DestoryKitchenObject(KitchenObject kitchenObject)
    {
        DestoryKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }
    [ServerRpc(RequireOwnership = false)] //标记成SeverRPC客户端调用它时，代码不会在客户端执行，而是发送消息给服务器执行  如果owner为true，就只有服务器自己能调用，客户端一按就被拒
    public void DestoryKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetWorkReference)
    {
        kitchenObjectNetWorkReference.TryGet(out NetworkObject kitchenObjectNetWorkObject);
        KitchenObject kitchenObject = kitchenObjectNetWorkObject.GetComponent<KitchenObject>();
        if (kitchenObject == null)
            return;
        kitchenObject.Destorykitchenobject();
        ClearKitchenObjectOnParentClientRpc(kitchenObject.NetworkObject);
    }

    [ClientRpc]
    public void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetWorkReference)
    {
        kitchenObjectNetWorkReference.TryGet(out NetworkObject kitchenObjectNetWorkObject);
        KitchenObject KitchenObject = kitchenObjectNetWorkObject.GetComponent<KitchenObject>();
        KitchenObject.ClearKitchenObjectOnParent();

    }


    public bool ColorIsAvailable(int colorid)
    {
        foreach (var playerdata in playerDataList)
        {
            if (playerdata.colorId == colorid)  //说明这个颜色已经被其他玩家所使用 
            {
                return false;
            }
        }
        return true;
    }
    public void ColorChanged(int colorid )
    {
        ColorChangedServerRpc(colorid);
    }
    [ServerRpc(RequireOwnership =false)]
    public void ColorChangedServerRpc(int colorid, ServerRpcParams serverRpcParams=default)
    {
        if (!ColorIsAvailable(colorid))
        {
            return;
        }
        PlayerData playerdata = playerDataList[GetIndexFromClient(serverRpcParams.Receive.SenderClientId)];
        playerdata.colorId = colorid;
        playerDataList[GetIndexFromClient(serverRpcParams.Receive.SenderClientId)]= playerdata;
    }

    public void ExitPlayer(ulong clientID)
    {
        NetworkManager.Singleton.DisconnectClient(clientID);
        Debug.Log("111");
        NetworkManager_OnClientExitCallback(clientID);
    }
}
