using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LivingThingStatusEffect : MonoBehaviourPun
{
    List<CoreStatusEffect> coreStatusEffects = new List<CoreStatusEffect>();
    private LivingThing livingThing;

    public float totalSpeedAmount { get; private set; }
    public float totalHasteAmount { get; private set; }
    public float totalSlowAmount { get; private set; }

    private Transform model;
    public float modelOffsetSpeed = 5f;
    public float modelOffsetMultiplier = 2f;
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        model = transform.Find("Model");

    }

    private void Update()
    {
        Vector3 offset = model.transform.localPosition;
        offset.y = Mathf.MoveTowards(offset.y, modelOffset * modelOffsetMultiplier, modelOffsetSpeed * Time.deltaTime);
        model.transform.localPosition = offset;
    }

    public void ApplyCoreStatusEffect(CoreStatusEffect ce)
    {
        int uid = Random.Range(int.MinValue, int.MaxValue);
        while(RetrieveCoreStatusEffect(uid) != null) // Just in case the uid generation threads the needle:
        {
            uid = Random.Range(int.MinValue, int.MaxValue); // Reroll.
        }
        ce.uid = uid;
        ce.owner = livingThing;
        coreStatusEffects.Add(ce);
        print("ApplyCoreStatusEffect");
        StatusEffectParticleEffectManager.instance.CreateParticleEffect(ce);

        photonView.RPC("RpcApplyCoreStatusEffect", RpcTarget.Others, uid, ce.caster.photonView.ViewID, (byte)ce.type, ce.duration, ce.parameter);
    }

    public void CleanseCoreStatusEffect(CoreStatusEffectType type)
    {
        photonView.RPC("RpcCleanseCoreStatusEffect", RpcTarget.All, (byte)type);
    }

    public void RemoveCoreStatusEffect(CoreStatusEffect ce)
    {
        if (ce.isAboutToBeDestroyed) return;
        ce.isAboutToBeDestroyed = true;
        photonView.RPC("RpcRemoveCoreStatusEffect", RpcTarget.All, ce.uid);
    }

    public void AddDurationToCoreStatusEffect(CoreStatusEffect ce, float duration)
    {
        photonView.RPC("RpcAddDurationToCoreStatusEffect", RpcTarget.All, ce.uid, duration);
    }

    public void SetDurationOfCoreStatusEffect(CoreStatusEffect ce, float duration)
    {
        photonView.RPC("RpcSetDurationOfCoreStatusEffect", RpcTarget.All, ce.uid, duration);
    }


    public bool IsAffectedBy(CoreStatusEffectType type) // TODO: Cache this!
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.type == type) return true;
        }
        return false;
    }

    public CoreStatusEffect RetrieveCoreStatusEffect(int uid)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.uid == uid) return ce;
        }
        return null;
    }


    private bool wasStunned = false;
    private float modelOffset = 0;
    private void FixedUpdate()
    {
        bool canTick = SelfValidator.CanTick.Evaluate(livingThing);
        bool tickedAirborne = false;
        float remainingAirboneDuration = 0;
        List<CoreStatusEffect> removeList = new List<CoreStatusEffect>();
        totalHasteAmount = 0;
        totalSlowAmount = 0;
        totalSpeedAmount = 0;

        foreach (CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.isAboutToBeDestroyed) continue;
            if (!canTick && ce.type != CoreStatusEffectType.Stasis) continue;
            if (ce.type == CoreStatusEffectType.Airborne)
            {
                remainingAirboneDuration += ce.duration;
                if (!tickedAirborne)
                {
                    ce.duration = Mathf.MoveTowards(ce.duration, 0, Time.deltaTime);
                    tickedAirborne = true;
                }
            }
            else
            {
                ce.duration = Mathf.MoveTowards(ce.duration, 0, Time.deltaTime);
            }

            if (ce.type == CoreStatusEffectType.Haste && ce.parameter != null)
            {
                totalHasteAmount += (float)ce.parameter;
            }
            if (ce.type == CoreStatusEffectType.Slow && ce.parameter != null)
            {
                totalSlowAmount += (float)ce.parameter;
            }
            if (ce.type == CoreStatusEffectType.Speed && ce.parameter != null)
            {
                totalSpeedAmount += (float)ce.parameter;
            }

            if (photonView.IsMine)
            {
                if (ce.duration <= 0 || (!SelfValidator.CanHaveHarmfulCoreStatusEffects.Evaluate(livingThing) && ce.IsHarmful()))
                {
                    removeList.Add(ce);
                }
            }

        }
        foreach (CoreStatusEffect ce in removeList)
        {
            RemoveCoreStatusEffect(ce);
        }

        if (photonView.IsMine)
        {
            bool isAffectedByStun = IsAffectedBy(CoreStatusEffectType.Stun);
            if (!wasStunned && isAffectedByStun)
            {
                wasStunned = true;
                photonView.RPC("RpcStartStunned", RpcTarget.All);
            }
            else if (wasStunned && !isAffectedByStun)
            {
                wasStunned = false;
                photonView.RPC("RpcStopStunned", RpcTarget.All);
            }

        }

        modelOffset = remainingAirboneDuration;
    }




    [PunRPC]
    public void RpcApplyCoreStatusEffect(int uid, int casterViewID, byte type, float duration, object parameter)
    {
        LivingThing caster = PhotonNetwork.GetPhotonView(casterViewID).GetComponent<LivingThing>();
        LivingThing owner = livingThing;

        CoreStatusEffect ce = new CoreStatusEffect(caster, (CoreStatusEffectType)type, duration, parameter);
        ce.owner = livingThing;
        ce.uid = uid;
        coreStatusEffects.Add(ce);

        StatusEffectParticleEffectManager.instance.CreateParticleEffect(ce);
        print("Apply");

    }

    [PunRPC]
    public void RpcRemoveCoreStatusEffect(int uid)
    {
        for(int i = 0; i < coreStatusEffects.Count; i++)
        {
            if (coreStatusEffects[i].uid == uid)
            {
                coreStatusEffects[i].duration = 0;
                coreStatusEffects[i].isAboutToBeDestroyed = true;
                coreStatusEffects.RemoveAt(i);
                break;
            }
        }

    }

    [PunRPC]
    public void RpcAddDurationToCoreStatusEffect(int uid, float duration)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.uid == uid)
            {
                ce.duration += duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcSetDurationOfCoreStatusEffect(int uid, float duration)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(ce.uid == uid)
            {
                ce.duration = duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcCleanseCoreStatusEffect(byte type)
    {
        for (int i = 0; i < coreStatusEffects.Count; i++)
        {
            if (coreStatusEffects[i].type == (CoreStatusEffectType)type)
            {
                coreStatusEffects[i].duration = 0;
                coreStatusEffects[i].isAboutToBeDestroyed = true;
                coreStatusEffects.RemoveAt(i);
                RpcCleanseCoreStatusEffect(type);
                break;
            }
        }

    }

    [PunRPC]
    private void RpcStartStunned()
    {
        livingThing.OnStartStunned.Invoke();
    }

    [PunRPC]
    private void RpcStopStunned()
    {
        livingThing.OnStopStunned.Invoke();
    }
}
