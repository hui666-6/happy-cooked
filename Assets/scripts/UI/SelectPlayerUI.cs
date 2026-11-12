using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectPlayerUI : NetworkBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject ReadyText;
    [SerializeField] private PlayerVisualUI playerVisualUI;
    [SerializeField] private Button exitButton;


    public void KickPlayer()
    {
        PlayerData playerdata = KitchenGameMultiPlayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        Debug.Log(playerIndex);
        KitchenGameMultiPlayer.Instance.ExitPlayer(playerdata.clientID);
        Debug.Log(playerdata.clientID);
    }


    private void Start()
    {
        UpdatePlayer();
        exitButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        KitchenGameMultiPlayer.Instance.OnPlayerDataListChanged += KitchenGamaMultiPlayer_OnPlayerDataListChanged;
        CharacterReady.Instacne.OnPlayerReadyChanged += CharacterReady_OnPlayerReadyChanged;

    }

    private void CharacterReady_OnPlayerReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void KitchenGamaMultiPlayer_OnPlayerDataListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (KitchenGameMultiPlayer.Instance.IsClientPlayerID(playerIndex))
        {
            PlayerData playerdata = KitchenGameMultiPlayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            ReadyText.SetActive(CharacterReady.Instacne.IsReady(playerdata.clientID));
            Show();
            playerVisualUI.SetPlayerColor(KitchenGameMultiPlayer.Instance.GetColor(playerdata.colorId));
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        KitchenGameMultiPlayer.Instance.OnPlayerDataListChanged -= KitchenGamaMultiPlayer_OnPlayerDataListChanged;
        CharacterReady.Instacne.OnPlayerReadyChanged -= CharacterReady_OnPlayerReadyChanged;
    }

}
