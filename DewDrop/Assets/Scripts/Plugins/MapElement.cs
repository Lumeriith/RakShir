using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public abstract class MapElement : MonoBehaviourPun
{
    public bool isActivated { private set; get; }
    public bool isFinished { private set; get; }

    protected abstract void OnActivate(bool isLocal);

    public void Activate()
    {
        if (isActivated) return;
        isFinished = false;
        isActivated = true;
        OnActivate(true);
        photonView.RPC("RpcRemoteActivate", RpcTarget.Others);
    }

    [PunRPC]
    protected void RpcRemoteActivate()
    {
        isFinished = false;
        isActivated = true;
        OnActivate(false);
    }

    public void MarkAsFinished(bool state = true)
    {
        photonView.RPC("RpcMarkAsFinished", RpcTarget.All, state);
    }

    [PunRPC]
    protected void RpcMarkAsFinished(bool state)
    {
        isFinished = state;
    }

}
