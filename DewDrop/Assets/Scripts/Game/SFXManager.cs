using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


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



    public static SFXInstance CreateSFXInstance(string sfxName, Vector3 position, bool onlyPlayOnLocal = false)
    {
        SFXInstance sfx = PhotonNetwork.Instantiate("SFXInstances/" + sfxName, position, Quaternion.identity).GetComponent<SFXInstance>();
        if (onlyPlayOnLocal)
        {
            sfx.photonView.RPC("RpcStop", RpcTarget.Others);
        }
        return sfx;
    }
    public static SFXInstance CreateSFXInstance(string sfxName, Vector3 position, Photon.Realtime.Player targetPlayer)
    {
        SFXInstance sfx = PhotonNetwork.Instantiate("SFXInstances/" + sfxName, position, Quaternion.identity).GetComponent<SFXInstance>();
        sfx.photonView.RPC("RpcStop", RpcTarget.All);
        sfx.photonView.RPC("RpcPlay", targetPlayer);
        return sfx;
    }






}
