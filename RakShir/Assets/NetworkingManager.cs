using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public const byte event_SyncAllStats = 1;

    private float lastSyncAllStats = 0;
    public float syncAllStatsInterval = 5f;

    private RaiseEventOptions raiseToAll = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    private SendOptions sendReliably = new SendOptions { Reliability = true };

    void Update()
    {
        if(!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

       
        if (Time.time - lastSyncAllStats > syncAllStatsInterval)
        {
            lastSyncAllStats = Time.time;
            PhotonNetwork.RaiseEvent(event_SyncAllStats, null, raiseToAll, sendReliably);
        }
    }

    
}
