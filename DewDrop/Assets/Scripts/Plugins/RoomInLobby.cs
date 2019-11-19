using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomInLobby : MonoBehaviour, IPointerClickHandler
{
    private LobbyView view;
    private Text text_RoomName;

    public int index;
    private void Awake()
    {
        view = transform.parent.parent.parent.GetComponent<LobbyView>();
        text_RoomName = transform.Find("Room Name").GetComponent<Text>();
    }

    private void Update()
    {
        if (view.roomList.Count <= index) return;
        text_RoomName.text = view.roomList[index].Name;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left) return;
        view.JoinRoom(index);
        
    }
}
