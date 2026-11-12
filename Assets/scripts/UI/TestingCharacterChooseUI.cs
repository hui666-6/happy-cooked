using UnityEngine;
using UnityEngine.UI;

public class TestingCharacterChooseUI : MonoBehaviour
{
    [SerializeField] public Button readyButton; 

    private void Start()
    {
        readyButton.onClick.AddListener(() =>
        {
            CharacterReady.Instacne.SetPlayerReady();
        }
        );
    }
}
