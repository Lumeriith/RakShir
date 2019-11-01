using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LivingThingStatusEffect : MonoBehaviourPun
{
    private const float overTimeEffectTickInterval = 0.5f;

    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    private LivingThing livingThing;

    public float totalSpeedAmount { get; private set; }
    public float totalHasteAmount { get; private set; }
    public float totalSlowAmount { get; private set; }

    public float totalHealOverTimeAmount { get; private set; }
    public float totalDamageOverTimeAmount { get; private set; }
    public float totalShieldAmount { get; private set; }

    private Transform model;
    public float modelOffsetSpeed = 3f;
    public float modelOffsetMultiplier = 2.5f;

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

    public List<StatusEffect> GetStatusEffectsByType(StatusEffectType type)
    {
        List<StatusEffect> result = new List<StatusEffect>();
        foreach (StatusEffect ce in statusEffects)
        {
            if (ce.type == type)
            {
                result.Add(ce);
            }
        }
        return result;
    }

    public List<StatusEffect> GetCustomStatusEffectsByName(string name)
    {
        List<StatusEffect> result = new List<StatusEffect>();
        foreach (StatusEffect ce in statusEffects)
        {
            if (ce.type == StatusEffectType.Custom && (string)ce.parameter == name)
            {
                result.Add(ce);
            }
        }
        return result;
    }

    public void ApplyStatusEffect(StatusEffect ce)
    {
        int uid = Random.Range(int.MinValue, int.MaxValue);
        while(GetStatusEffectByUID(uid) != null) // Just in case the uid generation threads the needle:
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

    public void SetParameterOfStatusEffect(StatusEffect ce, object parameter)
    {
        photonView.RPC("RpcSetParameterOfStatusEffect", RpcTarget.All, ce.uid, parameter);
    }

    public bool IsAffectedBy(StatusEffectType type) // TODO: Cache this!
    {
        foreach(StatusEffect ce in statusEffects)
        {
            if (ce.type == type) return true;
        }
        return false;
    }

    public StatusEffect GetStatusEffectByUID(int uid)
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




        if (Time.time - lastOverTimeEffectTickTime > overTimeEffectTickInterval)
        {
            lastOverTimeEffectTickTime += overTimeEffectTickInterval;
            doOverTimeEffectTicks = true;
            lastOverTimeEffectTickTime = Time.time;
        }

        List<float> reservedHealAmounts = new List<float>();
        List<LivingThing> reservedHealCasters = new List<LivingThing>();

        List<float> reservedMagicDamageAmounts = new List<float>();
        List<LivingThing> reservedMagicDamageCasters = new List<LivingThing>();


        foreach (StatusEffect ce in statusEffects)
        {
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

            if (doOverTimeEffectTicks)
            {
                if (ce.type == StatusEffectType.HealOverTime)
                {
                    if(ce.duration == 0)
                    {
                        if (photonView.IsMine)
                        {
                            reservedHealAmounts.Add((float)ce.parameter);
                            reservedHealCasters.Add(ce.caster);
                            //ce.caster.DoHeal((float)ce.parameter, livingThing, true);
                            removeList.Add(ce);
                        }
                    }
                    else
                    {
                        float amount = Mathf.Min((float)ce.parameter, (float)ce.parameter / ce.duration * overTimeEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            reservedHealAmounts.Add(amount);
                            reservedHealCasters.Add(ce.caster);
                            //ce.caster.DoHeal(amount, livingThing);
                        }
                        ce.parameter = (float)ce.parameter - amount;
                    }
                }
                else if (ce.type == StatusEffectType.DamageOverTime)
                {
                    if (ce.duration == 0)
                    {
                        if (photonView.IsMine)
                        {
                            reservedMagicDamageAmounts.Add((float)ce.parameter);
                            reservedMagicDamageCasters.Add(ce.caster);
                            //ce.caster.DoMagicDamage((float)ce.parameter, livingThing, true);
                        }
                        removeList.Add(ce);
                    }
                    else
                    {
                        float amount = Mathf.Min((float)ce.parameter, (float)ce.parameter / ce.duration * overTimeEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            reservedMagicDamageAmounts.Add(amount);
                            reservedMagicDamageCasters.Add(ce.caster);
                            //ce.caster.DoMagicDamage(amount, livingThing, true);
                        }
                        ce.parameter = (float)ce.parameter - amount;
                    }
                }
            }


            if (photonView.IsMine)
            {
                if ((ce.duration <= 0 && ce.type != StatusEffectType.HealOverTime && ce.type != StatusEffectType.DamageOverTime) || (!SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(livingThing) && ce.IsHarmful()) || livingThing.IsDead())
                {
                    removeList.Add(ce);
                }
            }

        }

        for (int i = 0; i < reservedHealAmounts.Count; i++)
        {
            reservedHealCasters[i].DoHeal(reservedHealAmounts[i], livingThing, true);
        }

        for (int i = 0; i < reservedMagicDamageAmounts.Count; i++)
        {
            reservedMagicDamageCasters[i].DoMagicDamage(reservedMagicDamageAmounts[i], livingThing, true);
        }



        totalHasteAmount = 0;
        totalSlowAmount = 0;
        totalSpeedAmount = 0;
        totalHealOverTimeAmount = 0;
        totalDamageOverTimeAmount = 0;
        totalShieldAmount = 0;

        foreach (StatusEffect ce in statusEffects)
        {
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

            if (ce.type == StatusEffectType.Shield)
            {
                totalShieldAmount += (float)ce.parameter;
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

    public void ApplyShieldDamage(float amount)
    {
        photonView.RPC("RpcApplyShieldDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    public void RpcApplyShieldDamage(float amount)
    {
        float remainingAmount = amount;
        foreach(StatusEffect ce in statusEffects)
        {
            if(ce.type == StatusEffectType.Shield)
            {
                if((float)ce.parameter >= remainingAmount)
                {
                    ce.parameter = (float)ce.parameter - remainingAmount;
                    break;
                }
                else
                {
                    remainingAmount -= (float)ce.parameter;
                    ce.parameter = 0f;
                    ce.duration = 0f;
                }
            }
        }
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
    }

    [PunRPC]
    public void RpcRemoveStatusEffect(int uid)
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].uid == uid)
            {
                statusEffects[i].duration = 0;
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
    public void RpcSetParameterOfStatusEffect(int uid, object parameter)
    {
        foreach (StatusEffect ce in statusEffects)
        {
            if (ce.uid == uid)
            {
                ce.parameter = parameter;
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
