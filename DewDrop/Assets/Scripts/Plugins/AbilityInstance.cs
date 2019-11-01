using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum DetachBehaviour { DontStop, StopEmitting, StopEmittingAndClear }



[RequireComponent(typeof(PhotonView))]
public abstract class AbilityInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    protected bool isCreated { get; private set; } = false;
    protected bool isDestroyed { get; private set; } = false;

    protected bool isAlive { get { return isCreated && !isDestroyed; } }
    public CastInfo info;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        CastInfo castInfo;
        if(initData == null)
        {
            print("AbilityInstance must be instantiated by AbilityInstanceManager!");
            return;
        }
        if ((int)initData[0] != -1)
        {
            castInfo.owner = PhotonNetwork.GetPhotonView((int)initData[0]).GetComponent<LivingThing>();
        }
        else
        {
            castInfo.owner = null;
        }
        castInfo.point = (Vector3)initData[1];
        castInfo.directionVector = (Vector3)initData[2];

        if ((int)initData[3] != -1)
        {
            castInfo.target = PhotonNetwork.GetPhotonView((int)initData[3]).GetComponent<LivingThing>();
        }
        else
        {
            castInfo.target = null;
        }

        object[] data = new object[initData.Length - 4];
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = initData[i + 4];
        }

        isCreated = true;
        this.info = castInfo;
        OnCreate(castInfo, data);
    }

    protected abstract void OnCreate(CastInfo castInfo, object[] data);

    protected virtual void AliveUpdate() { }

    private void Update()
    {
        if (isCreated && !isDestroyed) AliveUpdate();
    }

    public void DestroySelf()
    {
        if (!isCreated || isDestroyed) return;
        isDestroyed = true;
        photonView.RPC("RpcDestroySelf",RpcTarget.AllViaServer);
    }

    [PunRPC]
    protected void RpcDestroySelf()
    {
        isDestroyed = true;
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }

    }


    public void DetachChildParticleSystemsAndAutoDelete(DetachBehaviour behaviour = DetachBehaviour.DontStop)
    {
        photonView.RPC("RpcDetachChildParticleSystemsAndAutoDelete", RpcTarget.All, (int)behaviour);
    }

    [PunRPC]
    protected void RpcDetachChildParticleSystemsAndAutoDelete(int clear)
    {
        ParticleSystem[] psList = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psList)
        {
            if(clear != 0) ps.Stop(false, clear == 2 ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            ps.gameObject.AddComponent<ParticleSystemAutoDestroy>();
            ps.transform.parent = transform.parent;
        }
    }


}
