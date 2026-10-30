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
    }
} 
