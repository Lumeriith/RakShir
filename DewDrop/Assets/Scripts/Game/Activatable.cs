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

    protected abstract void OnChannelStart(LivingThing activator);
    protected abstract void OnChannelCancel(LivingThing activator);
    protected abstract void OnChannelSuccess(LivingThing activator);


    private List<KeyValuePair<LivingThing, Channel>> dict = new List<KeyValuePair<LivingThing, Channel>>();

    protected override void Start()
    {
        base.Start();
        GameManager.instance.OnActivatableInstantiate.Invoke(this);
    }

    public void StartActivate(LivingThing activator)
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
        dict.Add(new KeyValuePair<LivingThing, Channel>(activator, newChannel));

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
        OnChannelStart(PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>());
    }

    [PunRPC]
    protected void RpcChannelSuccess(int viewId)
    {
        OnChannelSuccess(PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>());
    }

    [PunRPC]
    protected void RpcChannelCancel(int viewId)
    {
        LivingThing activator = PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>();
        if (activator.photonView.IsMine) activator.OnDealDamage -= Interrupt;
        OnChannelCancel(activator);
        
    }


}
