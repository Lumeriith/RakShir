using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LivingThingStatusEffect : MonoBehaviourPun
{
    private const float overTimeEffectTickInterval = 0.5f;

    List<StatusEffect> statusEffects = new List<StatusEffect>();
    private LivingThing livingThing;

    public float totalSpeedAmount { get; private set; }
    public float totalHasteAmount { get; private set; }
    public float totalSlowAmount { get; private set; }

    public float totalHealOverTimeAmount { get; private set; }
    public float totalDamageOverTimeAmount { get; private set; }

    private Transform model;
    public float modelOffsetSpeed = 5f;
    public float modelOffsetMultiplier = 2f;

    private float lastOverTimeEffectTickTime = 0f;
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        model = transform.Find("Model");
        lastOverTimeEffectTickTime = Time.time;

    }

    private void Update()
    {
        Vector3 offset = model.transform.localPosition;
        offset.y = Mathf.MoveTowards(offset.y, modelOffset * modelOffsetMultiplier, modelOffsetSpeed * Time.deltaTime);
        model.transform.localPosition = offset;
    }

    public void ApplyStatusEffect(StatusEffect ce)
    {
        int uid = Random.Range(int.MinValue, int.MaxValue);
        while(RetrieveStatusEffect(uid) != null) // Just in case the uid generation threads the needle:
        {
            uid = Random.Range(int.MinValue, int.MaxValue); // Reroll.
        }
        ce.uid = uid;
        ce.owner = livingThing;
        statusEffects.Add(ce);
        StatusEffectParticleEffectManager.instance.CreateParticleEffect(ce);

        photonView.RPC("RpcApplyStatusEffect", RpcTarget.Others, uid, ce.caster.photonView.ViewID, (byte)ce.type, ce.duration, ce.parameter);
    }

    public void CleanseStatusEffect(StatusEffectType type)
    {
        photonView.RPC("RpcCleanseStatusEffect", RpcTarget.All, (byte)type);
    }

    public void RemoveStatusEffect(StatusEffect ce)
    {
        if (ce.isAboutToBeDestroyed) return;
        ce.isAboutToBeDestroyed = true;
        photonView.RPC("RpcRemoveStatusEffect", RpcTarget.All, ce.uid);
    }

    public void AddDurationToStatusEffect(StatusEffect ce, float duration)
    {
        photonView.RPC("RpcAddDurationToStatusEffect", RpcTarget.All, ce.uid, duration);
    }

    public void SetDurationOfStatusEffect(StatusEffect ce, float duration)
    {
        photonView.RPC("RpcSetDurationOfStatusEffect", RpcTarget.All, ce.uid, duration);
    }


    public bool IsAffectedBy(StatusEffectType type) // TODO: Cache this!
    {
        foreach(StatusEffect ce in statusEffects)
        {
            if (ce.type == type) return true;
        }
        return false;
    }

    public StatusEffect RetrieveStatusEffect(int uid)
    {
        foreach(StatusEffect ce in statusEffects)
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
        bool doOverTimeEffectTicks = false;
        float remainingAirboneDuration = 0;

        List<StatusEffect> removeList = new List<StatusEffect>();

        totalHasteAmount = 0;
        totalSlowAmount = 0;
        totalSpeedAmount = 0;
        totalHealOverTimeAmount = 0;
        totalDamageOverTimeAmount = 0;

        if (Time.time - lastOverTimeEffectTickTime > overTimeEffectTickInterval)
        {
            lastOverTimeEffectTickTime += overTimeEffectTickInterval;
            doOverTimeEffectTicks = true;
            lastOverTimeEffectTickTime = Time.time;
        }

        foreach (StatusEffect ce in statusEffects)
        {
            if (ce.isAboutToBeDestroyed) continue;
            if (!canTick && ce.type != StatusEffectType.Stasis) continue;
            if (ce.type == StatusEffectType.Airborne)
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

            if (ce.type == StatusEffectType.Haste && ce.parameter != null)
            {
                totalHasteAmount += (float)ce.parameter;
            }
            if (ce.type == StatusEffectType.Slow && ce.parameter != null)
            {
                totalSlowAmount += (float)ce.parameter;
            }
            if (ce.type == StatusEffectType.Speed && ce.parameter != null)
            {
                totalSpeedAmount += (float)ce.parameter;
            }

            if (ce.type == StatusEffectType.HealOverTime)
            {
                totalHealOverTimeAmount += (float)ce.parameter;
            }

            if (ce.type == StatusEffectType.DamageOverTime)
            {
                totalDamageOverTimeAmount += (float)ce.parameter;
            }


            if (doOverTimeEffectTicks)
            {
                if (ce.type == StatusEffectType.HealOverTime)
                {
                    if(ce.duration == 0)
                    {
                        if (photonView.IsMine)
                        {
                            ce.caster.DoHeal((float)ce.parameter, livingThing);
                            removeList.Add(ce);
                        }
                    }
                    else
                    {
                        float amount = Mathf.Min((float)ce.parameter, (float)ce.parameter / ce.duration * overTimeEffectTickInterval);
                        if (photonView.IsMine) ce.caster.DoHeal(amount, livingThing);
                        ce.parameter = (float)ce.parameter - amount;
                    }
                }
                else if (ce.type == StatusEffectType.DamageOverTime)
                {
                    if (ce.duration == 0)
                    {
                        if (photonView.IsMine) ce.caster.DoMagicDamage((float)ce.parameter, livingThing);
                        removeList.Add(ce);
                    }
                    else
                    {
                        float amount = Mathf.Min((float)ce.parameter, (float)ce.parameter / ce.duration * overTimeEffectTickInterval);
                        if (photonView.IsMine) ce.caster.DoMagicDamage(amount, livingThing);
                        ce.parameter = (float)ce.parameter - amount;
                    }
                }
            }


            if (photonView.IsMine)
            {
                if ((ce.duration <= 0 && ce.type != StatusEffectType.HealOverTime && ce.type != StatusEffectType.DamageOverTime) || (!SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(livingThing) && ce.IsHarmful()))
                {
                    removeList.Add(ce);
                }
            }

        }
        foreach (StatusEffect ce in removeList)
        {
            RemoveStatusEffect(ce);
        }

        if (photonView.IsMine)
        {
            bool isAffectedByStun = IsAffectedBy(StatusEffectType.Stun);
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
    public void RpcApplyStatusEffect(int uid, int casterViewID, byte type, float duration, object parameter)
    {
        LivingThing caster = PhotonNetwork.GetPhotonView(casterViewID).GetComponent<LivingThing>();
        LivingThing owner = livingThing;

        StatusEffect ce = new StatusEffect(caster, (StatusEffectType)type, duration, parameter);
        ce.owner = livingThing;
        ce.uid = uid;
        statusEffects.Add(ce);

        StatusEffectParticleEffectManager.instance.CreateParticleEffect(ce);
        print("Apply");

    }

    [PunRPC]
    public void RpcRemoveStatusEffect(int uid)
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].uid == uid)
            {
                statusEffects[i].duration = 0;
                statusEffects[i].isAboutToBeDestroyed = true;
                statusEffects.RemoveAt(i);
                break;
            }
        }

    }

    [PunRPC]
    public void RpcAddDurationToStatusEffect(int uid, float duration)
    {
        foreach(StatusEffect ce in statusEffects)
        {
            if (ce.uid == uid)
            {
                ce.duration += duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcSetDurationOfStatusEffect(int uid, float duration)
    {
        foreach(StatusEffect ce in statusEffects)
        {
            if(ce.uid == uid)
            {
                ce.duration = duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcCleanseStatusEffect(byte type)
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == (StatusEffectType)type)
            {
                statusEffects[i].duration = 0;
                statusEffects[i].isAboutToBeDestroyed = true;
                statusEffects.RemoveAt(i);
                RpcCleanseStatusEffect(type);
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
