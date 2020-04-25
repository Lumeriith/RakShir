using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public abstract class Activatable : DewActionCaller
{
    public float activationRange = 2.5f;
    public Channel channel;
    public bool isInterruptedByDamage = true;

    protected abstract void OnChannelStart(Entity activator);
    protected abstract void OnChannelCancel(Entity activator);
    protected abstract void OnChannelSuccess(Entity activator);


    private List<KeyValuePair<Entity, Channel>> dict = new List<KeyValuePair<Entity, Channel>>();

    protected override void Start()
    {
        base.Start();
        GameManager.instance.OnActivatableInstantiate.Invoke(this);
    }

    public void StartActivate(Entity activator)
    {
        if (!activator.photonView.IsMine)
        {
            Debug.LogError("StartActivate() can be only called with local LivingThing!");
            return;
        }

        photonView.RPC("RpcChannelStart", RpcTarget.All, activator.photonView.ViewID);

        Channel newChannel = new Channel(channel.channelValidator, channel.duration, channel.canMove, channel.canAttack, channel.canUseAbility, channel.canBeCanceledByCaster, 
            ()=> { photonView.RPC("RpcChannelSuccess", RpcTarget.All, activator.photonView.ViewID); },
            ()=> { photonView.RPC("RpcChannelCancel", RpcTarget.All, activator.photonView.ViewID); });
        activator.control.StartChanneling(newChannel);

        activator.OnDealDamage += Interrupt;
        dict.Add(new KeyValuePair<Entity, Channel>(activator, newChannel));

    }

    public void Interrupt(InfoDamage info)
    {
        for(int i = 0; i < dict.Count; i++)
        {
            if(dict[i].Key == info.to)
            {
                dict[i].Value.Cancel();
                dict.RemoveAt(i);
                break;
            }
            
        }
    }


    [PunRPC]
    protected void RpcChannelStart(int viewId)
    {
        OnChannelStart(PhotonNetwork.GetPhotonView(viewId).GetComponent<Entity>());
    }

    [PunRPC]
    protected void RpcChannelSuccess(int viewId)
    {
        OnChannelSuccess(PhotonNetwork.GetPhotonView(viewId).GetComponent<Entity>());
    }

    [PunRPC]
    protected void RpcChannelCancel(int viewId)
    {
        Entity activator = PhotonNetwork.GetPhotonView(viewId).GetComponent<Entity>();
        if (activator.photonView.IsMine) activator.OnDealDamage -= Interrupt;
        OnChannelCancel(activator);
        
    }


}
