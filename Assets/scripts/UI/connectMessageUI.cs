using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class connectMessageUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI message;
    [SerializeField] public Button cancelButton;


    private void Awake()
    {
        cancelButton.onClick.AddListener(Hide);
    }
    private void Start()
    {
        KitchenGameMultiPlayer.Instance.OnFailedToJoinGame += KitchenGameMultiPlayer_OnFailedToJoinGame;
        Hide();
    }

    private void KitchenGameMultiPlayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Show();
        message.text = NetworkManager.Singleton.DisconnectReason;
        if (message.text == "")
        {
            message.text = "Failed to connect";
        }
    }



    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        KitchenGameMultiPlayer.Instance.OnFailedToJoinGame -= KitchenGameMultiPlayer_OnFailedToJoinGame;
    }
}
