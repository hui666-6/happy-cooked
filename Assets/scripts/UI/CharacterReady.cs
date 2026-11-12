using System;
using System.Collections.Generic;
using Unity.Netcode;


public class CharacterReady : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyDictionary;
    public  static  CharacterReady Instacne { get; private set; }

    public event EventHandler OnPlayerReadyChanged;

    private void Awake()
    {
        if (Instacne != null&&Instacne!=this)
        {
           Destroy(Instacne);
            return;
        }
            Instacne = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public  void SetPlayerReady()
    {
        SetPlayerReadyChangedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public  void SetPlayerReadyChangedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        SetPlayerReadyChangedClientRpc(serverRpcParams.Receive.SenderClientId);
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                return;
            }
        }
        Loader.LoadNetworkScence(Loader.scene.GameScene);
    }

    [ClientRpc]
    public void SetPlayerReadyChangedClientRpc(ulong clientID)
    {
        playerReadyDictionary[clientID] = true; 
        OnPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsReady(ulong clientID)
    {
        return playerReadyDictionary.ContainsKey(clientID)&&playerReadyDictionary[clientID];
    }
}
 