using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public abstract class Item : Activatable
{
    [Header("Metadata Settings")]
    public Sprite itemIcon;
    public string itemName;
    public ItemTier itemTier;
    [ResizableTextArea]
    public string itemDescription;

    [HideInInspector]
    public LivingThing owner = null;



    public void TransferOwnership(LivingThing owner)
    {
        photonView.RPC("RpcTransferOwnership", RpcTarget.All, owner.photonView.ViewID);
    }

    public void Disown()
    {
        if (owner == null) return;
        photonView.RPC("RpcDisown", RpcTarget.All);
    }

    protected override void OnChannelCancel(LivingThing activator)
    {

    }

    protected override void OnChannelStart(LivingThing activator)
    {

    }

    protected override void OnChannelSuccess(LivingThing activator)
    {
        if (activator.photonView.IsMine)
        {
            activator.GetComponent<PlayerItemBelt>().Pickup(this);
        }
    }


    [PunRPC]
    protected void RpcTransferOwnership(int owner_id)
    {
        LivingThing livingThing = PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();

        if (owner != null)
        {
            RpcDisown();
        }

        owner = livingThing;
        transform.SetParent(owner.transform);
        transform.position = owner.transform.position;
        gameObject.SetActive(false);
    }


    [PunRPC]
    protected void RpcDisown()
    {
        if (owner == null) return;
        owner = null;
        transform.SetParent(null);
        gameObject.SetActive(true);
    }
}
