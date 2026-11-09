using UnityEngine;
using UnityEngine.UI;

public class LobbyScenceManager : MonoBehaviour
{
    [SerializeField] public Button creatGameButton;
    [SerializeField] public Button joinGameButoon;

    private void Start()
    {
        creatGameButton.onClick.AddListener(() =>
        {
            KitchenGameMultiPlayer.Instance.StartHost();
            Loader.LoadNetworkScence(Loader.scene.CharacterChooseScene);
        }
        );

        joinGameButoon.onClick.AddListener(() =>
        KitchenGameMultiPlayer.Instance.StartClient());
    }
}
