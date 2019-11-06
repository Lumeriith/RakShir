using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : Activatable
{
    private Room room;

    public bool isOpen;

    protected override void Start()
    {
        base.Start();
        room = transform.parent.parent.GetComponent<Room>();
        if (room == null) room = transform.parent.GetComponent<Room>();
    }

    private void Update()
    {
        isOpen = room.IsCleared();
    }

    protected override void OnChannelStart(LivingThing activator) { }
    protected override void OnChannelCancel(LivingThing activator) { }
    protected override void OnChannelSuccess(LivingThing activator)
    {
        Room nextRoom = room.nextRooms[Random.Range(0, room.nextRooms.Count)];
        if (activator.photonView.IsMine && isOpen)
        {
            activator.Teleport(nextRoom.entryPoint.position);
            activator.SetCurrentRoom(nextRoom);
        }

        
    }

}
