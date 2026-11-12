using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private int colorId;
    [SerializeField] private GameObject selectColor;
    [SerializeField] private Button SelectButton;

    private void Awake()
    {
        SelectButton.onClick.AddListener(() =>
        {
            KitchenGameMultiPlayer.Instance.ColorChanged(colorId);
        }
        );
    }
    private void Start()
    {
        KitchenGameMultiPlayer.Instance.OnPlayerDataListChanged += KitchenGameMultiPlayer_OnPlayerDataListChanged;
        SetImageColor();
        UpdatSelectedColor();
    }

    private void KitchenGameMultiPlayer_OnPlayerDataListChanged(object sender, System.EventArgs e)
    {
        UpdatSelectedColor();
    }

    private void SetImageColor()
    {
        image.color = KitchenGameMultiPlayer.Instance.GetColor(colorId);
    }

    //如果玩家身上的颜色ID与该颜色的ID一致的话 则UI显示为被选中状态
    private void UpdatSelectedColor()
    {
        if (KitchenGameMultiPlayer.Instance.GetPlayerDataFromClient(NetworkManager.Singleton.LocalClientId).colorId == colorId)
        {
            selectColor.SetActive(true);
        }
        else
        {
            selectColor.SetActive(false);
        }
    }
}
