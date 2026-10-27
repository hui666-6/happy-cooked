using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playeranimator : MonoBehaviour
{
   [SerializeField] private player player;
    private Animator anim;
    private const string ISWALKING = "iswalking";
    // Start is called before the first frame update
    void Start()
    {
       anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool(ISWALKING, player.Iswalking);

    }
}
