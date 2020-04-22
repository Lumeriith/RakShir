using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum DespawnBehaviour : byte { Immediately, WaitForParticleSystems, StopAndWaitForParticleSystems }
public enum AttachBehaviour : byte { Full, IgnoreRotation }

public class AbilityCallbacks
{
    public System.Action<InfoDamage> OnDealDamage = (InfoDamage _) => { };
    public System.Action<InfoDamage> OnDealPureDamage = (InfoDamage _) => { };
    public System.Action<InfoMagicDamage> OnDealMagicDamage = (InfoMagicDamage _) => { };
    public System.Action<InfoBasicAttackHit> OnDoBasicAttackHit = (InfoBasicAttackHit _) => { };
    public System.Action<InfoDeath> OnKill = (InfoDeath _) => { };
    public System.Action<InfoHeal> OnDoHeal = (InfoHeal _) => { };
    public System.Action<InfoManaHeal> OnDoManaHeal = (InfoManaHeal _) => { };
}

//[RequireComponent(typeof(PhotonView))]
public abstract class AbilityInstance : MonoBehaviourPun, IPunInstantiateMagicCallback, IDelayedDespawn
{
    public bool isAlive { get { return _isInitialized && !_isMarkedForDespawn; } }
    public bool isMine { get { return photonView.IsMine; } }
    public CastInfo info { get { return _info; } }

    public AbilityCallbacks callbacks { get; private set; }
    private CastInfo _info;
    public float creationTime { private set; get; }
    private bool _isReadyForDespawn;

    private bool _isInitialized;
    private bool _isMarkedForDespawn;

    private DespawnBehaviour _despawnBehaviour;

    private Transform _attachTo;
    private AttachBehaviour _attachBehaviour;
    private Vector3 _attachOffset;
    private Quaternion _attachRotation;
    private ParticleSystem _mainParticleSystem;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        _info = new CastInfo();

        if(initData == null)
        {
            print("AbilityInstance must be instantiated by AbilityInstanceManager!");
            return;
        }

        if ((int)initData[0] != -1)
        {
            _info.owner = PhotonNetwork.GetPhotonView((int)initData[0]).GetComponent<LivingThing>();
        }
        else
        {
            _info.owner = null;
            Debug.LogWarning("This AbilityInstance does not have an owner!\n" + this.name);
        }

        _info.point = (Vector3)initData[1];
        _info.directionVector = ((Vector3)initData[2]).normalized;

        if ((int)initData[3] != -1)
        {
            _info.target = PhotonNetwork.GetPhotonView((int)initData[3]).GetComponent<LivingThing>();
        }
        else
        {
            _info.target = null;
        }

        object[] data = new object[initData.Length - 4];
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = initData[i + 4];
        }

        _isReadyForDespawn = false;

        callbacks = new AbilityCallbacks();
        creationTime = Time.time;
        _isReadyForDespawn = false;
        _isInitialized = true;
        _isMarkedForDespawn = false;
        if (_mainParticleSystem == null) _mainParticleSystem = GetComponent<ParticleSystem>();

        _attachTo = null;

        OnCreate(this.info, data);
    }

    protected abstract void OnCreate(CastInfo info, object[] data);

    protected virtual void AliveUpdate() { }

    private void Update()
    {
        if (isAlive) AliveUpdate();
        if (_isMarkedForDespawn)
        {
            if(_attachTo != null)
            {
                if(_attachBehaviour == AttachBehaviour.Full)
                {
                    transform.position = _attachTo.TransformPoint(_attachOffset);
                    transform.rotation = _attachTo.rotation * _attachRotation;
                }
                else if (_attachBehaviour == AttachBehaviour.IgnoreRotation)
                {
                    transform.position = _attachTo.TransformPoint(_attachOffset);
                }
            }

            if (_despawnBehaviour == DespawnBehaviour.Immediately) _isReadyForDespawn = true;
            else if (_despawnBehaviour == DespawnBehaviour.WaitForParticleSystems) _isReadyForDespawn = _mainParticleSystem == null || !_mainParticleSystem.IsAlive();
            else if (_despawnBehaviour == DespawnBehaviour.StopAndWaitForParticleSystems) _isReadyForDespawn = _mainParticleSystem == null || !_mainParticleSystem.IsAlive();
            else _isReadyForDespawn = true;
        }
    }

    public void Despawn(DespawnBehaviour behaviour)
    {
        if (!isAlive) return;
        photonView.RPC("RpcDespawn", RpcTarget.All, (byte)behaviour);
    }

    public void Despawn(DespawnBehaviour behaviour, MonoBehaviourPun attachTo, AttachBehaviour attachBehaviour)
    {
        if (!isAlive) return;
        Vector3 attachOffset = attachTo.transform.InverseTransformPoint(transform.position);
        Quaternion attachRotation = Quaternion.Inverse(attachTo.transform.rotation) * transform.rotation;
        photonView.RPC("RpcDespawnWithAttach", RpcTarget.All, (byte)behaviour, attachTo.photonView.ViewID, (byte)attachBehaviour, attachOffset, attachRotation);
    }

    [PunRPC]
    protected void RpcDespawn(byte behaviour)
    {
        _despawnBehaviour = (DespawnBehaviour)behaviour;
        if (_despawnBehaviour == DespawnBehaviour.StopAndWaitForParticleSystems) _mainParticleSystem.Stop();
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    protected void RpcDespawnWithAttach(byte behaviour, int attachToViewID, byte attachBehaviour, Vector3 attachOffset, Quaternion attachRotation)
    {
        _despawnBehaviour = (DespawnBehaviour)behaviour;
        _attachTo = PhotonView.Find(attachToViewID)?.transform;
        _attachBehaviour = (AttachBehaviour)attachBehaviour;
        _attachOffset = attachOffset;
        _attachRotation = attachRotation;

        transform.position = _attachTo.TransformPoint(_attachOffset);
        transform.rotation = _attachTo.rotation * _attachRotation;

        if (_despawnBehaviour == DespawnBehaviour.StopAndWaitForParticleSystems) _mainParticleSystem.Stop();
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    protected void RpcDoEvent(string eventString)
    {
        OnReceiveEvent(eventString);
    }

    protected virtual void OnReceiveEvent(string eventString) { }

    public bool IsReadyForDespawn()
    {
        return _isReadyForDespawn;

    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
