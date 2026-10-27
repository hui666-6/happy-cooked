using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class playersounds : MonoBehaviour
{
    private player player;
    private float soundroate = 0.15f;
    private float soundtimer = 0;
    private void Start()
    {
      player = gameObject.GetComponent<player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.Iswalking)
        {
            soundtimer += Time.deltaTime;
            if (soundtimer >= soundroate)
            { 
                soundtimer = 0;
                float volume = 0.2f;
                SoundManager.instance.stepsound(volume);
            }
        
        }
    }
}
