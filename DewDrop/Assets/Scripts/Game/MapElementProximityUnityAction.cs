using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
public class MapElementProximityUnityAction : MapElement
{
    public UnityEvent fireAction;
    public bool isObjective = true;

    protected override void OnActivate(bool isLocal)
    {
        if (isLocal && isObjective) GuidanceArrowManager.SetObjective(transform.position); 
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActivated || isFinished) return;
        Entity thing = other.GetComponent<Entity>();
        if (thing == null || !thing.photonView.IsMine || thing != GameManager.instance.localPlayer) return;
        MarkAsFinished();
        if (isObjective) GuidanceArrowManager.RemoveObjective();
        photonView.RPC("RpcDoFireAction", RpcTarget.All);
    }

    [PunRPC]
    private void RpcDoFireAction()
    {
        if(fireAction != null) fireAction.Invoke();
    }


}
