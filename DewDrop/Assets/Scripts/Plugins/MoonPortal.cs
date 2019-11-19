using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonPortal : Activatable
{
    private static Vector3 previousPlayerPosition;
    private static Room previousPlayerRoom = null;

    public Room targetRoom;

    protected override void OnChannelCancel(LivingThing activator) { }

    protected override void OnChannelStart(LivingThing activator) { }

    protected override void OnChannelSuccess(LivingThing activator)
    {
        if (!activator.photonView.IsMine) return;
        OverlayCanvas.Blink();
        if (previousPlayerRoom == null)
        {
            previousPlayerPosition = activator.transform.position;
            previousPlayerRoom = activator.currentRoom;
            if (activator.team == Team.Red && targetRoom.redCustomEntryPoint != null)
            {
                activator.Teleport(targetRoom.redCustomEntryPoint.position);
            }
            else if (activator.team == Team.Blue && targetRoom.blueCustomEntryPoint != null)
            {
                activator.Teleport(targetRoom.blueCustomEntryPoint.position);
            }
            else
            {
                activator.Teleport(targetRoom.entryPoint.position);
            }
            activator.SetCurrentRoom(targetRoom);
        }
        else
        {
            activator.Teleport(previousPlayerPosition);
            
            activator.SetCurrentRoom(previousPlayerRoom);
            previousPlayerRoom = null;
        }
    }
}
