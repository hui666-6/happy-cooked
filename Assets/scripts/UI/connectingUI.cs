using UnityEngine;

public class connectingUI : MonoBehaviour
{


    public void Start()
    {
        KitchenGameMultiPlayer.Instance.OnTryingToJoinGame += KitchenGameMultiPlayer_OnTryingToJoinGame;
        KitchenGameMultiPlayer.Instance.OnFailedToJoinGame += KitchenGameMultiPlayer_OnFailedToJoinGame;
        Hide();
    }

    private void KitchenGameMultiPlayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void KitchenGameMultiPlayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
      Show();
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
        KitchenGameMultiPlayer.Instance.OnTryingToJoinGame -= KitchenGameMultiPlayer_OnTryingToJoinGame;
        KitchenGameMultiPlayer.Instance.OnFailedToJoinGame -= KitchenGameMultiPlayer_OnFailedToJoinGame;
    }
}
