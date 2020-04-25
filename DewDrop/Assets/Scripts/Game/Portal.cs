using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
public class Portal : Activatable
{
    private Room room;

    public override Entity entity => null;

    private bool isOpen = false;

    public void OpenPortal()
    {
        isOpen = true;
        GuidanceArrowManager.SetObjective(transform.position);
    }

    protected override void Start()
    {
        base.Start();
        room = transform.parent.parent.GetComponent<Room>();
        if (room == null) room = transform.parent.GetComponent<Room>();
    }



    protected override void OnChannelStart(Entity activator) { }
    protected override void OnChannelCancel(Entity activator) { }
    protected override void OnChannelSuccess(Entity activator)
    {
        if (activator.photonView.IsMine && isOpen)
        {
            GameEventMessage.SendEvent("Map Obelisk");
        }

        
    }

}
