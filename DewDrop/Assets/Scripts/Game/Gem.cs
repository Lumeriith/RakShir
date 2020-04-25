using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public abstract class Gem : Item, IDewActionCaller
{
    [Header("Gem Settings")]
    public AbilityTrigger trigger;
    public int level = 0;
    public int maxLevel = 4;

    private List<AbilityInstanceSafeReference> _references = new List<AbilityInstanceSafeReference>();

    private GameObject deactivatedModel;
    private List<Collider> deactivatedColliders = new List<Collider>();
    private List<Rigidbody> deactivatedRigidbodies = new List<Rigidbody>();

    public LivingThing invokerEntity { get => owner; }
    public Action<InfoManaSpent> OnSpendMana { get; set; } = (_) => { };
    public Action<InfoDamage> OnDealDamage { get; set; } = (_) => { };
    public Action<InfoDamage> OnDealPureDamage { get; set; } = (_) => { };
    public Action<InfoMagicDamage> OnDealMagicDamage { get; set; } = (_) => { };
    public Action<InfoBasicAttackHit> OnDoBasicAttackHit { get; set; } = (_) => { };
    public Action<InfoHeal> OnDoHeal { get; set; } = (_) => { };
    public Action<InfoManaHeal> OnDoManaHeal { get; set; } = (_) => { };

    public AbilityInstanceSafeReference CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, object[] data = null)
    {
        CastInfo info = new CastInfo { owner = owner };
        return CreateAbilityInstance(prefabName, position, rotation, info, data);
    }

    public AbilityInstanceSafeReference CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo info, object[] data = null)
    {
        PurgeReferencesList();
        AbilityInstanceSafeReference reference = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, data);
        _references.Add(reference);
        return reference;
    }

    private void PurgeReferencesList()
    {
        for (int i = _references.Count - 1; i >= 0; i--)
        {
            if (!_references[i].isValid) _references.RemoveAt(i);
        }
    }

    public bool IsAnyInstanceActive()
    {
        PurgeReferencesList();
        return _references.Count != 0;
    }

    public AbilityInstanceSafeReference GetLastInstance()
    {
        PurgeReferencesList();
        if (_references.Count == 0) return null;
        return _references[_references.Count - 1];
    }

    public AbilityInstanceSafeReference GetFirstInstance()
    {
        PurgeReferencesList();
        if (_references.Count == 0) return null;
        return _references[0];
    }



    public void SendEventToAbilityInstance(string eventString, AbilityInstanceEventTargetType target)
    {
        if (!IsAnyInstanceActive())
        {
            Debug.LogWarning("There is no active AbilityInstance to send event to!\n" + name);
            return;
        }
        switch (target)
        {
            case AbilityInstanceEventTargetType.EveryInstance:
                for (int i = 0; i < _references.Count; i++)
                {
                    _references[i].Dereference().photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                }
                break;
            case AbilityInstanceEventTargetType.FirstInstance:
                _references[0].Dereference().photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
            case AbilityInstanceEventTargetType.LastInstance:
                _references[_references.Count - 1].Dereference().photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
        }
    }

    public virtual void OnEquip(LivingThing owner, AbilityTrigger trigger) { }

    public virtual void OnUnequip(LivingThing owner, AbilityTrigger trigger) { }

    public virtual void OnTriggerCast(bool isMine) { }

    public virtual void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstanceSafeReference reference) { }

    public virtual void AliveUpdate(bool isMine) { }

    private void Update()
    {
        if (owner != null && trigger != null) AliveUpdate(owner.photonView.IsMine);
    }

    public void SetLevel(int level)
    {
        photonView.RPC("RpcSetLevel", RpcTarget.All, level);
    }

    public void Equip(string triggerName)
    {
        photonView.RPC("RpcEquip", RpcTarget.All, triggerName);
    }

    public void Unequip()
    {
        photonView.RPC("RpcUnequip", RpcTarget.All);
    }

    // Reactivates gem that has been deactivated due to the connected equipment being unequipped.
    public void Reactivate()
    {
        photonView.RPC("RpcReactivate", RpcTarget.All);
    }

    // Deactivates gem due to the connected equipment being unequipped.
    public void Deactivate()
    {
        photonView.RPC("RpcDeactivate", RpcTarget.All);
    }

    [PunRPC]
    protected void RpcOnAbilityInstanceCreatedFromTrigger(int referenceViewID)
    {
        OnAbilityInstanceCreatedFromTrigger(false, AbilityInstanceSafeReference.RetrieveOrCreate(referenceViewID));
    }


    [PunRPC]
    protected void RpcReactivate()
    {
        OnEquip(owner, trigger);
    }

    [PunRPC]
    protected void RpcDeactivate()
    {
        OnUnequip(owner, trigger);
    }

    [PunRPC]
    protected void RpcEquip(string triggerName)
    {
        trigger = owner.transform.Find(triggerName).GetComponent<AbilityTrigger>();
        trigger.connectedGems.Add(this);
        transform.parent = trigger.transform;

        Transform t = transform.Find("Model");
        if (t != null)
        {
            deactivatedModel = t.gameObject;
            deactivatedModel.SetActive(false);
        }

        deactivatedColliders.AddRange(GetComponents<Collider>());
        deactivatedRigidbodies.AddRange(GetComponents<Rigidbody>());
        for (int i = deactivatedColliders.Count - 1; i >= 0; i--)
        {
            if (!deactivatedColliders[i].enabled) deactivatedColliders.RemoveAt(i);
            else deactivatedColliders[i].enabled = false;
        }
        for (int i = deactivatedRigidbodies.Count - 1; i >= 0; i--)
        {
            if (deactivatedRigidbodies[i].isKinematic) deactivatedRigidbodies.RemoveAt(i);
            else
            {
                deactivatedRigidbodies[i].isKinematic = true;
                deactivatedRigidbodies[i].detectCollisions = false;
            }
        }


        gameObject.SetActive(true);

        OnEquip(owner, trigger);
    }

    [PunRPC]
    protected void RpcUnequip()
    {
        OnUnequip(owner, trigger);
        trigger.connectedGems.Remove(this);
        trigger = null;
        transform.parent = owner.transform;

        for (int i = 0; i < deactivatedColliders.Count; i++)
        {
            deactivatedColliders[i].enabled = true;
        }
        for (int i = 0; i < deactivatedRigidbodies.Count; i++)
        {
            deactivatedRigidbodies[i].isKinematic = false;
            deactivatedRigidbodies[i].detectCollisions = true;
        }


        if (deactivatedModel != null) deactivatedModel.gameObject.SetActive(true);

        deactivatedColliders.Clear();
        deactivatedModel = null;

        gameObject.SetActive(false);
    }


    [PunRPC]
    protected void RpcSetLevel(int level)
    {
        this.level = level;
    }

    [PunRPC]
    protected virtual void RpcOnTriggerCast()
    {
        OnTriggerCast(false);
    }

    public int Serialize() => photonView.ViewID;
}
