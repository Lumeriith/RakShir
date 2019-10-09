using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarManager : MonoBehaviour
{
    private static AvatarManager _instance;
    public static AvatarManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AvatarManager>();
            }
            return _instance;
        }
    }

    private GameObject elemental;
    private GameObject reptile;

    private void Awake()
    {
        elemental = transform.Find("Elemental").gameObject;
        reptile = transform.Find("Reptile").gameObject;
    }

    public void SetAvatar(PlayerType type)
    {
        if(type == PlayerType.Elemental)
        {
            elemental.SetActive(true);
            reptile.SetActive(false);
        }
        else
        {
            elemental.SetActive(false);
            reptile.SetActive(true);
        }
    }

}
