using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SFXManager : MonoBehaviour
{
    public static SFXManager instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<SFXManager>();
            return _instance;
        }
    }
    private static SFXManager _instance;

    private Camera main;

    private void Start()
    {
        main = Camera.main;
    }

    private void Update()
    {
        if (GameManager.instance.localPlayer != null) transform.position = GameManager.instance.localPlayer.transform.position;
        transform.rotation = main.transform.rotation;
    }

}
