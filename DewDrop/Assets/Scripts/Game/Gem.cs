using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public abstract class Gem : Item
{
    public bool isGemActivated { get; private set; }

    [Header("Gem Settings")]
    public AbilityTrigger trigger;
    public int level = 0;
    public int maxLevel = 4;

    private List<AbilityInstance> _instances = new List<AbilityInstance>();

    public AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, object[] data = null)
    {
        CastInfo info = new CastInfo { owner = owner };
        return CreateAbilityInstance(prefabName, position, rotation, info, data);
    }

    public AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo info, object[] data = null)
    {
        PurgeReferencesList();
        AbilityInstance reference = AbilityInstanceManager.CreateAbilityInstanceFromGem(prefabName, position, rotation, info, this, data);
        _instances.Add(reference);
        return reference; 
    }

    private void PurgeReferencesList()
    {
        for (int i = _instances.Count - 1; i >= 0; i--)
        {
            if (!_instances[i].isAlive) _instances.RemoveAt(i);
        }
    }

    public bool IsAnyInstanceActive()
    {
        PurgeReferencesList();
        return _instances.Count != 0;
    }

    public AbilityInstance GetLastInstance()
    {
        PurgeReferencesList();
        if (_instances.Count == 0) return null;
        return _instances[_instances.Count - 1];
    }

    public AbilityInstance GetFirstInstance()
    {
        PurgeReferencesList();
        if (_instances.Count == 0) return null;
        return _instances[0];
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
                for (int i = 0; i < _instances.Count; i++)
                {
                    _instances[i].SendEvent(eventString);
                }
                break;
            case AbilityInstanceEventTargetType.FirstInstance:
                _instances[0].SendEvent(eventString);
                break;
            case AbilityInstanceEventTargetType.LastInstance:
                _instances[_instances.Count - 1].SendEvent(eventString);
                break;
        }
    }

    public virtual void OnGemActivate(Entity owner, AbilityTrigger trigger) { }

    public virtual void OnGemDeactivate(Entity owner, AbilityTrigger trigger) { }

    public virtual void OnTriggerCast(bool isMine) { }

    public virtual void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstance instance) { }

    public virtual void AliveUpdate(bool isMine) { }

    private void Update()
    {
        if (owner != null && trigger != null) AliveUpdate(owner.photonView.IsMine);
    }

    public void SetLevel(int level)
    {
        photonView.RPC(nameof(RpcSetLevel), RpcTarget.All, level);
    }

    public void Equip(string triggerName)
    {
        photonView.RPC(nameof(RpcEquip), RpcTarget.All, triggerName);
    }

    public void Unequip()
    {
        photonView.RPC(nameof(RpcUnequip), RpcTarget.All);
    }

    public void ActivateGem()
    {
        photonView.RPC(nameof(RpcActivateGem), RpcTarget.All);
    }

    public void DeactivateGem()
    {
        photonView.RPC(nameof(RpcDeactivateGem), RpcTarget.All);
    }

    [PunRPC]
    protected void RpcOnAbilityInstanceCreatedFromTrigger(int viewID)
    {
        OnAbilityInstanceCreatedFromTrigger(false, PhotonNetwork.GetPhotonView(viewID).GetComponent<AbilityInstance>());
    }


    [PunRPC]
    protected void RpcActivateGem()
    {
        OnGemActivate(owner, trigger);
    }

    [PunRPC]
    protected void RpcDeactivateGem()
    {
        OnGemDeactivate(owner, trigger);
    }

    [PunRPC]
    protected void RpcEquip(string triggerName)
    {
        trigger = owner.transform.Find(triggerName).GetComponent<AbilityTrigger>();
        trigger.connectedGems.Add(this);
        transform.parent = trigger.transform;

        if (trigger.equipment.isEquipped)
        {
            isGemActivated = true;
            OnGemActivate(owner, trigger);
        }
    }

    [PunRPC]
    protected void RpcUnequip()
    {
        trigger.connectedGems.Remove(this);
        trigger = null;
        transform.parent = owner.transform;
        
        if(isGemActivated) OnGemDeactivate(owner, trigger);
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

    public override InfoTextIcon infoTextIcon => InfoTextIcon.Gem;
}
