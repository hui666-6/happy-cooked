using Unity.Netcode;
using UnityEngine;

public class ClearStaticData : MonoBehaviour
{
    private void Start()
    {
       TrashCounter.ClearStaticdata();
       BaseCounter.ClearStaticData();
        player.ClearStaticData();
        CuttingCounter.clearStaticData();
        platekitchenobject.ClearStaticData();
        if (KitchenGameMultiPlayer.Instance != null)
        {
            Destroy(KitchenGameMultiPlayer.Instance);
        }
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton);
        }
    }
} 
