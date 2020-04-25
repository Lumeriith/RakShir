using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public static NetworkingManager instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<NetworkingManager>();
            return _instance;
        }
    }
    private static NetworkingManager _instance;


    private float lastSyncAllStats = 0;
    public float syncAllStatsInterval = 5f;

    private RaiseEventOptions raiseToAll = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    private SendOptions sendReliably = new SendOptions { Reliability = true };

    private static bool registeredCustomTypes = false;

    private void Start()
    {
        if (!registeredCustomTypes)
        {
            //PhotonPeer.RegisterType(typeof(SourceInfo), 255, SourceInfo.SerializeSourceInfo, SourceInfo.DeserializeSourceInfo);
            registeredCustomTypes = true;
        }
    }



    

    void Update()
    {
        if(!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

       
        if (Time.time - lastSyncAllStats > syncAllStatsInterval)
        {
            
        }
    }


    




}
