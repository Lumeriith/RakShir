using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Marker : MonoBehaviour
{
    Camera main;

    private void Awake()
    {
        main = Camera.main;
    }


    void Update()
    {
        transform.rotation = main.transform.rotation;
    }


    public static void CreateMarker(string name, Vector3 position)
    {
        PhotonNetwork.Instantiate(name, )
    }

}
