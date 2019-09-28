using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class NetworkTest : MonoBehaviourPunCallbacks
{
    public string testLevelName;
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "3";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        PhotonNetwork.CreateRoom(null, roomOptions);
        Debug.Log("Creating a room.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room.");
        if (PhotonNetwork.IsMasterClient)
        {
            //PhotonNetwork.LoadLevel(testLevelName);
        }

        GameManager.instance.localPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero + Vector3.up * 5, Quaternion.identity).GetComponent<LivingThing>();
        UnitControlManager.instance.selectedUnit = GameManager.instance.localPlayer;
        
    }
}
