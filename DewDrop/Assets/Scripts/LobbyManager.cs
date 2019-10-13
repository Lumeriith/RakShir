using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public enum GameType { OnlineTestGame }
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static GameType gameType = GameType.OnlineTestGame;
    public static bool properlyConfiguredGame = false;
    public TextMeshProUGUI statusText;
    public byte maxPlayersForOnlineTestGame = 10;
    public string gameVersion = "8";


    private void Start()
    {
        if (!properlyConfiguredGame)
        {
            Debug.Log("Lobby Scene cannot be loaded on its own. Returning to Main Menu...");
            SceneManager.LoadScene("Main Menu");
            return;
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        if (gameType == GameType.OnlineTestGame)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "Connecting to Master Server";
        }
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Master.\nJoining Random Room...";
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Join Random Room Failed.\n" + message;
        if(gameType == GameType.OnlineTestGame)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersForOnlineTestGame });
            statusText.text += "\nCreating a Room...";
        }

    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Test Game");
        }
    }
}
