using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;

public abstract class Item : Activatable
{
    [Header("Metadata Settings")]
    public Sprite itemIcon;
    public string itemName;
    public ItemTier itemTier;
    public float value;
    [MultiLineProperty]
    public string itemDescription;

    [HideInInspector]
    public Entity owner = null;

    private Vector3 startPosition;

    public override Entity entity => owner;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.drag = 1f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    private void FixedUpdate()
    {

        if(transform.position.y < startPosition.y - 15f)
        {
            GetComponent<Rigidbody>().velocity = Vector3.up * 2f; // QoL change. Fallen items will come back.
            transform.position = startPosition + Vector3.up;
        }

    }

    public void TransferOwnership(Entity owner)
    {
        photonView.RPC("RpcTransferOwnership", RpcTarget.All, owner.photonView.ViewID);
    }

    public void Disown()
    {
        if (owner == null) return;
        photonView.RPC("RpcDisown", RpcTarget.All);
    }

    public void DestroySelf()
    {
        photonView.RPC("RpcDestroySelf", photonView.Owner);
    }

    [PunRPC]
    public void RpcDestroySelf()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    protected override void OnChannelCancel(Entity activator)
    {

    }

    protected override void OnChannelStart(Entity activator)
    {

    }

    protected override void OnChannelSuccess(Entity activator)
    {
        if (activator.photonView.IsMine)
        {
            activator.GetComponent<PlayerInventory>().Pickup(this);
        }
    }


    [PunRPC]
    protected void RpcTransferOwnership(int owner_id)
    {
        Entity livingThing = PhotonNetwork.GetPhotonView(owner_id).GetComponent<Entity>();

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
        transform.position = owner.transform.position + owner.GetCenterOffset();
        startPosition = transform.position;
        owner = null;
        transform.SetParent(null);
        gameObject.SetActive(true);
        SFXManager.CreateSFXInstance("si_local_ItemDrop", transform.position, true);
    }
}
