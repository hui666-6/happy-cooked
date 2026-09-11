using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSceneUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI Score;
    public void Update()
    {
      int count = OrderManager.Instance.GetCurrentScore();
      Score.text = ("·ÖÊý£º"+count);
    }
}

    

