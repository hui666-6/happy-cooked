using System.Collections.Generic;
using Unity.Netcode;

public class CharacterReady : NetworkBehaviour
{
    private Dictionary<ulong, bool> playerReadyDictionary;
    public  static  CharacterReady Instacne { get; private set; }

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
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                return;
            }
        }
        Loader.LoadNetworkScence(Loader.scene.GameScene);
    }
}
