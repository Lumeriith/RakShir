using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MoonPortal : Activatable
{
    private static Vector3 previousPlayerPosition;
    private static Room previousPlayerRoom = null;

    public bool isOnByDefault = false;

    private Room targetRoom;

    protected override void Start()
    {
        base.Start();
        gameObject.SetActive(isOnByDefault);
    }
    protected override void OnChannelCancel(LivingThing activator) { }

    protected override void OnChannelStart(LivingThing activator) { }

    protected override void OnChannelSuccess(LivingThing activator)
    {
        if (!activator.photonView.IsMine) return;
        if (targetRoom == null) targetRoom = transform.Find("/Map/Lunar Altar(Clone)").GetComponent<Room>();
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


    public void OpenPortal()
    {
        photonView.RPC("RpcTogglePortal", RpcTarget.All, true);
    }


    public void ClosePortal()
    {
        photonView.RPC("RpcTogglePortal", RpcTarget.All, false);
    }

    [PunRPC]
    private void RpcTogglePortal(bool toggle)
    {
        gameObject.SetActive(toggle);
    }
}
