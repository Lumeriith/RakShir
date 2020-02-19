using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum DetachBehaviour { DontStop, StopEmitting, StopEmittingAndClear }



//[RequireComponent(typeof(PhotonView))]
public abstract class AbilityInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    protected bool isCreated { get; private set; } = false;
    protected bool isDestroyed { get; private set; } = false;

    public bool isAlive { get { return isCreated && !isDestroyed; } }
    public bool isMine { get { return photonView.IsMine; } }

    public CastInfo info;

    public SourceInfo source;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        this.info = new CastInfo();
        this.source = new SourceInfo();

        if(initData == null)
        {
            print("AbilityInstance must be instantiated by AbilityInstanceManager!");
            return;
        }

        if ((int)initData[0] != -1)
        {
            this.info.owner = PhotonNetwork.GetPhotonView((int)initData[0]).GetComponent<LivingThing>();
        }
        else
        {
            this.info.owner = null;
            Debug.LogWarning("This AbilityInstance does not have an owner!\n" + this.name);
        }

        this.info.point = (Vector3)initData[1];
        this.info.directionVector = ((Vector3)initData[2]).normalized;

        if ((int)initData[3] != -1)
        {
            this.info.target = PhotonNetwork.GetPhotonView((int)initData[3]).GetComponent<LivingThing>();
        }
        else
        {
            this.info.target = null;
        }

        if ((string)initData[4] != "" && this.info.owner != null)
        {
            foreach(Transform t in this.info.owner.transform)
            {
                if(t.name == (string)initData[4])
                {
                    source.trigger = t.GetComponent<AbilityTrigger>();
                    break;
                }
            }
        }

        if((int)initData[5] != -1)
        {
            source.gem = PhotonNetwork.GetPhotonView((int)initData[5]).GetComponent<Gem>();
        }

        source.instance = this;
        source.thing = this.info.owner;

        object[] data = new object[initData.Length - 6];
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = initData[i + 6];
        }

        isCreated = true;
        OnCreate(this.info, data);

        //Debug.Log(string.Format("AbilityInstance - {0}\nSource Trigger: {1}\nSource Gem: {2}\nSource Instance: {3}", this, source.trigger, source.gem, source.instance));
    }

    protected abstract void OnCreate(CastInfo info, object[] data);

    protected virtual void AliveUpdate() { }

    private void Update()
    {
        if (isCreated && !isDestroyed) AliveUpdate();
    }

    public void DestroySelf()
    {
        if (!isCreated || isDestroyed) return;
        isDestroyed = true;
        photonView.RPC("RpcDestroySelf", RpcTarget.All);
    }

    public void DestroySelf(float delay)
    {
        if (!isCreated || isDestroyed) return;

        StartCoroutine(CoroutineDelayedDestroySelf(delay));

    }

    private IEnumerator CoroutineDelayedDestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDestroyed = true;
        photonView.RPC("RpcDestroySelf", RpcTarget.All);
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

    [PunRPC]
    protected void RpcDoEvent(string eventString)
    {
        OnReceiveEvent(eventString);
    }

    protected virtual void OnReceiveEvent(string eventString) { }

    public void DetachChildParticleSystemsAndAutoDelete(DetachBehaviour behaviour = DetachBehaviour.DontStop, MonoBehaviourPun attachTo = null)
    {
        photonView.RPC("RpcDetachChildParticleSystemsAndAutoDelete", RpcTarget.All, (int)behaviour, attachTo == null ? -1 : attachTo.photonView.ViewID);
    }

    [PunRPC]
    protected void RpcDetachChildParticleSystemsAndAutoDelete(int clear, int viewID)
    {
        PhotonView view = viewID != -1 ? PhotonNetwork.GetPhotonView(viewID) : null;
        ParticleSystem[] psList = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psList)
        {
            if(clear != 0) ps.Stop(false, clear == 2 ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            ps.gameObject.AddComponent<ParticleSystemAutoDestroy>();
            if (view != null) ps.transform.parent = view.transform;
            else ps.transform.parent = transform.parent;
        }
    }

}
