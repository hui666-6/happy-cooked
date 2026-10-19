using UnityEngine;

public class ClearStaticData : MonoBehaviour
{
    private void Start()
    {
        TrashCounter.ClearStaticData();
        BaseCounter.ClearStaticData();
        player.ClearStaticData();
        CuttingCounter.clearStaticData();
        platekitchenobject.ClearStaticData();
    }
}
