using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
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
        if (activator.photonView.IsMine && isOpen)
        {
            GameEventMessage.SendEvent("Map Obelisk");
        }

        
    }

}
