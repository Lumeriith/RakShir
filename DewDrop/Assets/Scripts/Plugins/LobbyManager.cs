using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public enum GameType { Gladiator, Playground }
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static GameType gameType = GameType.Playground;
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
        if (gameType == GameType.Gladiator)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "Connecting to Master Server";
        } else if (gameType == GameType.Playground)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
        }

        Music.Play("Silence");
    }


    public override void OnConnectedToMaster()
    {
        if(gameType == GameType.Gladiator)
        {
            statusText.text = "Connected to Master.\nJoining Random Room...";
            PhotonNetwork.JoinRandomRoom();
        }

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Join Random Room Failed.\n" + message;
        if(gameType == GameType.Gladiator)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
            statusText.text += "\nCreating a Room...";
        }

    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(gameType == GameType.Gladiator) PhotonNetwork.LoadLevel("Gladiator");
            else if (gameType == GameType.Playground) PhotonNetwork.LoadLevel("Playground");
        }
    }
}
