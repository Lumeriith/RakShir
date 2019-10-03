using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TestGame : MonoBehaviourPunCallbacks
{

    public List<Transform> spawnPoints;

    bool wasRedTeam = false;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "4";
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

        Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
        GameManager.instance.localPlayer = PhotonNetwork.Instantiate("Player", spawnPoint, Quaternion.identity).GetComponent<LivingThing>();

        PhotonNetwork.Instantiate("Equipments/equip_Armor_ElementalIntegrity", GameManager.instance.localPlayer.transform.position + Vector3.up * 3 + Random.onUnitSphere * 2f, Quaternion.identity);
        PhotonNetwork.Instantiate("Equipments/equip_Boots_ElementalDetermination", GameManager.instance.localPlayer.transform.position + Vector3.up * 3 + Random.onUnitSphere * 2f, Quaternion.identity);
        PhotonNetwork.Instantiate("Equipments/equip_Weapon_ElementalJustice", GameManager.instance.localPlayer.transform.position + Vector3.up * 3 + Random.onUnitSphere * 2f, Quaternion.identity);

        UnitControlManager.instance.selectedUnit = GameManager.instance.localPlayer;
        
    }
}
