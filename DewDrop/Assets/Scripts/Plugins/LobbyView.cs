using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyView : MonoBehaviourPunCallbacks
{
    private Text text_Status;
    private GameObject gobj_Layout;
    private GameObject gobj_RoomInLobby;
    private Button button_Back;

    private Transform transform_NewRoom;

    private List<RoomInLobby> roomInLobbies = new List<RoomInLobby>();
    public List<RoomInfo> roomList = new List<RoomInfo>();
    public string gameVersion = "f0";

    private void Awake()
    {
        gobj_Layout = transform.Find("Rooms Panel/Layout").gameObject;
        text_Status = transform.Find("Rooms Panel/Status Text").GetComponent<Text>();
        transform_NewRoom = transform.Find("Rooms Panel/Layout/New Room");
        gobj_RoomInLobby = transform.Find("Rooms Panel/Layout/Room In Lobby").gameObject;
        button_Back = transform.Find("Button - Back").GetComponent<Button>();
        gobj_RoomInLobby.SetActive(false);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
        text_Status.text = "마스터 서버 연결 중...";
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    private void Update()
    {
        gobj_Layout.SetActive(PhotonNetwork.InLobby);
        button_Back.gameObject.SetActive(PhotonNetwork.IsConnectedAndReady || (PhotonNetwork.IsConnected && !PhotonNetwork.IsConnectedAndReady));
    }

    public override void OnConnectedToMaster()
    {
        text_Status.text = "로비 접속 중...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        text_Status.text = "방 목록 가져오는 중...";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        text_Status.text = "연결 해제됨!";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        text_Status.text = "";
        this.roomList = roomList;

        while(roomInLobbies.Count > roomList.Count)
        {
            Destroy(roomInLobbies[roomInLobbies.Count - 1].gameObject);
            roomList.RemoveAt(roomInLobbies.Count - 1);
        }

        while(roomInLobbies.Count < roomList.Count)
        {
            roomInLobbies.Add(Instantiate(gobj_RoomInLobby, gobj_Layout.transform).GetComponent<RoomInLobby>());
            roomInLobbies[roomInLobbies.Count - 1].gameObject.SetActive(true);
            roomInLobbies[roomInLobbies.Count - 1].index = roomInLobbies.Count - 1;
            roomInLobbies[roomInLobbies.Count - 1].transform.SetAsFirstSibling();
        }

    }

    public void StartRoom()
    {
        PhotonNetwork.CreateRoom(PlayerPrefs.GetString("characterName", "이름없는 영웅") + "님의 게임_" + Random.Range(100000, 1000000), new RoomOptions { MaxPlayers = 2 });
        text_Status.text += "방 만드는 중...";
    }

    public void JoinRoom(int roomIndex)
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;
        PhotonNetwork.JoinRoom(roomList[roomIndex].Name);
        text_Status.text = "방 연결 중...";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        text_Status.text = "방을 만들지 못했습니다! (" + returnCode + ")\n" + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        text_Status.text = "방에 연결하지 못했습니다! (" + returnCode + ")\n" + message;
    }

    public override void OnJoinedRoom()
    {
        text_Status.text = "맵 불러오는 중...";
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Gladiator");
        }
    }

}
