using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TsetNetWork : MonoBehaviour
{
   [SerializeField] private Button HostButton;
   [SerializeField] private Button ClientButton;

    void Awake()
    {
        HostButton.onClick.AddListener(() =>
        {
            Debug.Log("HostButton clicked");
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        ClientButton.onClick.AddListener(() =>
        {
            Debug.Log("ClientButton clicked");
            NetworkManager.Singleton.StartClient();
            Hide();
        });

    }
    private void Hide()
    {
      gameObject.SetActive(false);
    }
}
