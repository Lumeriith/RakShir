using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum MarkerAttachType { Bottom, Top, Center }


[RequireComponent(typeof(PhotonView))]
public class Marker : MonoBehaviourPun
{




    private Camera main;
    private new SkinnedMeshRenderer renderer;
    private Vector3 attachOffset;
    private MarkerAttachType attachType;
    private bool isVisibleToOwner = true;
    private bool isVisibleToEnemy = true;
    private bool isVisibleToAlly = true;
    private LivingThing owner;

    private void Awake()
    {
        main = Camera.main;
    }


    void Update()
    {
        transform.rotation = main.transform.rotation;
        if(renderer != null)
        {
            switch (attachType)
            {
                case MarkerAttachType.Bottom:
                    transform.position = renderer.bounds.center + renderer.bounds.extents.y * Vector3.down + attachOffset;
                    break;
                case MarkerAttachType.Center:
                    transform.position = renderer.bounds.center + attachOffset;
                    break;
                case MarkerAttachType.Top:
                    transform.position = renderer.bounds.center + renderer.bounds.extents.y * Vector3.up + attachOffset;
                    break;

            }
            
        }

    }

    
    public void AttachTo(LivingThing to, MarkerAttachType attachType, Vector3 offset)
    {
        photonView.RPC("RpcAttachTo", RpcTarget.All, to.photonView.ViewID, (int)attachType, offset);
    }

    public void SetPosition(Vector3 position)
    {
        photonView.RPC("RpcSetPosition", RpcTarget.All, position);
    }

    public void SetVisibility(bool isVisibleToOwner, bool isVisibleToEnemy, bool isVisibleToAlly)
    {
        photonView.RPC("RpcSetVisibility", RpcTarget.All, isVisibleToOwner, isVisibleToEnemy, isVisibleToAlly);
    }

    public void Destroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RpcAttachTo(int to_id, int attachType, Vector3 offset)
    {
        renderer = PhotonNetwork.GetPhotonView(to_id).transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>();
        this.attachType = (MarkerAttachType)attachType;
        this.attachOffset = offset;
    }

    [PunRPC]
    private void RpcSetPosition(Vector3 position)
    {
        transform.position = position;
    }


    [PunRPC]
    private void RpcSetVisibility(bool isVisibleToOwner, bool isVisibleToEnemy, bool isVisibleToAlly)
    {
        this.isVisibleToOwner = isVisibleToOwner;
        this.isVisibleToEnemy = isVisibleToEnemy;
        this.isVisibleToAlly = isVisibleToAlly;
    }

    [PunRPC]
    private void RpcSetOwner(int owner_id)
    {
        owner = PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();
    }

    public static Marker CreateMarker(string prefabName, Vector3 position, LivingThing owner)
    {
        Marker marker = PhotonNetwork.Instantiate(prefabName, position, Quaternion.identity).GetComponent<Marker>();
        marker.photonView.RPC("RpcSetOwner", RpcTarget.All, owner.photonView.ViewID);
        marker.SetPosition(position);
        return marker;
    }

}
