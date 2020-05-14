using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public enum DespawnBehaviour : byte { Immediately, WaitForParticleSystems, StopAndWaitForParticleSystems }
public enum AttachBehaviour : byte { Full, IgnoreRotation }

public abstract class DewActionCaller : MonoBehaviourPun
{
    private const int MaxStoredReferences = 5000;

    private static Dictionary<int, DewActionCaller> _storedReferences = new Dictionary<int, DewActionCaller>();
    private static Queue<int> _viewIDs = new Queue<int>();

    public abstract Entity entity { get; }

    public Action<InfoManaSpent> OnSpendMana { get; set; } = (_) => { };
    public Action<InfoDamage> OnDealDamage { get; set; } = (_) => { };
    public Action<InfoDamage> OnDealPureDamage { get; set; } = (_) => { };
    public Action<InfoMagicDamage> OnDealMagicDamage { get; set; } = (_) => { };
    public Action<InfoBasicAttackHit> OnDoBasicAttackHit { get; set; } = (_) => { };
    public Action<InfoHeal> OnDoHeal { get; set; } = (_) => { };
    public Action<InfoManaHeal> OnDoManaHeal { get; set; } = (_) => { };

    private int _uid;

    protected virtual void Start()
    {
        _uid = photonView.ViewID;
        _storedReferences.Add(_uid, this);
        _viewIDs.Enqueue(_uid);
        if (_viewIDs.Count > MaxStoredReferences) _storedReferences.Remove(_viewIDs.Dequeue());
    }


    public int GetActionCallerUID() => _uid;

    public static DewActionCaller Retrieve(int uid)
    {
        if (uid == 0) return null;
        return _storedReferences[uid];
    }
}


//[RequireComponent(typeof(PhotonView))]
public abstract class AbilityInstance : DewActionCaller, IPunInstantiateMagicCallback, IDelayedDestroy
{
    public bool isAlive { get { return _isInitialized && !_isMarkedForDespawn && this != null; } }
    public bool isMine { get { return photonView.IsMine; } }
    public CastInfo info { get { return _info; } }
    protected Gem gem { get; private set; }

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

    public override Entity entity => info.owner;

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
            _info.owner = PhotonNetwork.GetPhotonView((int)initData[0]).GetComponent<Entity>();
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
            _info.target = PhotonNetwork.GetPhotonView((int)initData[3]).GetComponent<Entity>();
        }
        else
        {
            _info.target = null;
        }

        if((int)initData[4] != -1)
        {
            gem = PhotonNetwork.GetPhotonView((int)initData[4]).GetComponent<Gem>();
        }
        else
        {
            gem = null;
        }


        object[] data = new object[initData.Length - 5];
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = initData[i + 5];
        }

        _isReadyForDespawn = false;

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

    /// <summary>
    /// Despawn this instance after the child ParticleSystems are fully stopped.
    /// </summary>
    public void Despawn()
    {
        if (!isAlive) return;
        photonView.RPC(nameof(RpcDespawn), RpcTarget.All, (byte)DespawnBehaviour.WaitForParticleSystems);
    }


    /// <summary>
    /// Despawn this instance using the given DespawnBehaviour.
    /// </summary>
    /// <param name="behaviour"></param>
    public void Despawn(DespawnBehaviour behaviour)
    {
        if (!isAlive) return;
        photonView.RPC(nameof(RpcDespawn), RpcTarget.All, (byte)behaviour);
    }

    /// <summary>
    /// Attach to the given target ignoring rotation and despawn after the child ParticleSystems are fully stopped.
    /// </summary>
    /// <param name="attachTo"></param>
    public void Despawn(MonoBehaviourPun attachTo)
    {
        Despawn(attachTo, AttachBehaviour.IgnoreRotation, DespawnBehaviour.WaitForParticleSystems);
    }

    /// <summary>
    /// Attach to the given target with the given behaviour and despawn after the child ParticleSystems are fully stopped.
    /// </summary>
    /// <param name="attachTo"></param>
    /// <param name="attachBehaviour"></param>
    /// <param name="behaviour"></param>
    public void Despawn(MonoBehaviourPun attachTo, AttachBehaviour attachBehaviour)
    {
        Despawn(attachTo, attachBehaviour, DespawnBehaviour.WaitForParticleSystems);
    }

    /// <summary>
    /// Attach to the given target ignoring rotation and despawn with the given behaviour.
    /// </summary>
    /// <param name="attachTo"></param>
    /// <param name="attachBehaviour"></param>
    /// <param name="behaviour"></param>
    public void Despawn(MonoBehaviourPun attachTo, DespawnBehaviour behaviour)
    {
        Despawn(attachTo, AttachBehaviour.IgnoreRotation, behaviour);
    }



    /// <summary>
    /// Attach to the given target with the given behaviour and despawn with the given behaviour.
    /// </summary>
    /// <param name="attachTo"></param>
    /// <param name="attachBehaviour"></param>
    /// <param name="behaviour"></param>
    public void Despawn(MonoBehaviourPun attachTo, AttachBehaviour attachBehaviour, DespawnBehaviour behaviour)
    {
        if (!isAlive) return;
        Vector3 attachOffset = attachTo.transform.InverseTransformPoint(transform.position);
        Quaternion attachRotation = Quaternion.Inverse(attachTo.transform.rotation) * transform.rotation;
        photonView.RPC(nameof(RpcDespawnWithAttach), RpcTarget.All, (byte)behaviour, attachTo.photonView.ViewID, (byte)attachBehaviour, attachOffset, attachRotation);
    }

    [PunRPC]
    protected void RpcDespawn(byte behaviour)
    {
        _despawnBehaviour = (DespawnBehaviour)behaviour;
        if (_despawnBehaviour == DespawnBehaviour.StopAndWaitForParticleSystems) _mainParticleSystem.Stop();
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        _isMarkedForDespawn = true;
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
        _isMarkedForDespawn = true;
    }

    public void SendEvent(string eventString)
    {
        photonView.RPC(nameof(RpcDoEvent), RpcTarget.All, eventString);
    }

    [PunRPC]
    protected void RpcDoEvent(string eventString)
    {
        OnReceiveEvent(eventString);
    }

    protected virtual void OnReceiveEvent(string eventString) { }

    public bool IsReadyForDestroy()
    {
        return _isReadyForDespawn;

    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Serialize() => photonView.ViewID;

    /// <summary>
    /// Instantiate the given particle effect and play at the position and destroy afterwards. This is only executed locally.
    /// </summary>
    /// <param name="particleEffect"></param>
    /// <param name="position"></param>
    public void PlayNew(GameObject particleEffect, Vector3 position)
    {
        PlayNew(particleEffect, position, Quaternion.identity);
    }

    /// <summary>
    /// Instantiate the given particle effect and play at the position with the given rotation and destroy afterwards. This is only executed locally.
    /// </summary>
    /// <param name="particleEffect"></param>
    /// <param name="position"></param>
    public void PlayNew(GameObject particleEffect, Vector3 position, Quaternion rotation)
    {
        GameObject newEffect = Instantiate(particleEffect, position, rotation);
        newEffect.GetComponent<ParticleSystem>().Play();
        newEffect.AddComponent<ParticleSystemAutoDestroy>();
    }
}
