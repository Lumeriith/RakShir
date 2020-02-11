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

    public float totalSpellPowerReductionAmount { get; private set; }

    public float totalSpellPowerBoostAmount { get; private set; }

    public float totalAttackDamageReductionAmount { get; private set; }
    
    public float totalAttackDamageBoostAmount { get; private set; }

    private float lastOverTimeEffectTickTime = 0f;

    private int[] statusEffectCountMap;

    private void Awake()
    {
        int maxStatusEffectTypeValue = 0;
        foreach(StatusEffectType type in System.Enum.GetValues(typeof(StatusEffectType)))
        {
            if ((int)type > maxStatusEffectTypeValue) maxStatusEffectTypeValue = (int)type;
        }
        statusEffectCountMap = new int[maxStatusEffectTypeValue + 1];
        livingThing = GetComponent<LivingThing>();
        lastOverTimeEffectTickTime = Time.time;
        
    }


    public List<StatusEffect> GetStatusEffectsByType(StatusEffectType type)
    {
        List<StatusEffect> result = new List<StatusEffect>();
        for (int i = 0;i<statusEffects.Count;i++)
        {
            if (statusEffects[i].type == type)
            {
                result.Add(statusEffects[i]);
            }
        }
        return result;
    }

    public List<StatusEffect> GetCustomStatusEffectsByName(string name)
    {
        List<StatusEffect> result = new List<StatusEffect>();
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == StatusEffectType.Custom && (string)statusEffects[i].parameter == name)
            {
                result.Add(statusEffects[i]);
            }
        }
        return result;
    }

    public void ApplyStatusEffect(StatusEffect ce)
    {
        if (ce.IsHarmful() && !SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(livingThing)) return;
        long uid; 
        do
        {
            uid = PhotonNetwork.LocalPlayer.ActorNumber + Random.Range(int.MinValue, int.MaxValue) << 32; // thank you ggsg ✨
            // it's probably okay to make guids sequentially generated, but better check the code thoroughly before doing it.
        } while (GetStatusEffectByUID(uid) != null);

        ce.uid = uid;
        ce.owner = livingThing;
        statusEffects.Add(ce);
        statusEffectCountMap[(int)ce.type] += 1;
        if(statusEffectCountMap[(int)ce.type] == 1) StatusEffectVisualsManager.CreateVisual(livingThing, ce.type);
        photonView.RPC("RpcApplyStatusEffect", RpcTarget.Others, uid, ce.caster.photonView.ViewID, (byte)ce.type, ce.duration, ce.parameter);
    }

    public void CleanseStatusEffect(StatusEffectType type)
    {
        photonView.RPC("RpcCleanseStatusEffect", RpcTarget.All, (byte)type);
    }

    public void CleanseAllHarmfulStatusEffects()
    {
        photonView.RPC("RpcCleanseAllHarmfulStatusEffects", RpcTarget.All);
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

    public bool IsAffectedBy(StatusEffectType type)
    {
        if(type == StatusEffectType.Dash) return livingThing.ongoingDisplacement != null && livingThing.ongoingDisplacement.isFriendly;
        if (type == StatusEffectType.Airborne) return livingThing.ongoingDisplacement != null && !livingThing.ongoingDisplacement.isFriendly;
        return statusEffectCountMap[(int)type] > 0;
    }

    public StatusEffect GetStatusEffectByUID(long uid)
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].uid == uid) return statusEffects[i];
        }
        return null;
    }


    private bool wasStunned = false;
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

        int temp = 0;


        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (!canTick && statusEffects[i].type != StatusEffectType.Stasis) continue;
            if (statusEffects[i].type == StatusEffectType.Airborne)
            {
                remainingAirboneDuration += statusEffects[i].duration;
                if (!tickedAirborne)
                {
                    statusEffects[i].duration = Mathf.MoveTowards(statusEffects[i].duration, 0, Time.deltaTime);
                    tickedAirborne = true;
                }
            }
            else
            {
                statusEffects[i].duration = Mathf.MoveTowards(statusEffects[i].duration, 0, Time.deltaTime);
            }

            if (doOverTimeEffectTicks)
            {
                if (statusEffects[i].type == StatusEffectType.HealOverTime)
                {
                    if(statusEffects[i].duration == 0)
                    {
                        if (photonView.IsMine)
                        {
                            temp = reservedHealCasters.IndexOf(statusEffects[i].caster);
                            if(temp != -1)
                            {
                                reservedHealAmounts[temp] += (float)statusEffects[i].parameter;
                            }
                            else
                            {
                                reservedHealAmounts.Add((float)statusEffects[i].parameter);
                                reservedHealCasters.Add(statusEffects[i].caster);
                            }

                            //ce.caster.DoHeal((float)ce.parameter, livingThing, true);
                            removeList.Add(statusEffects[i]);
                        }
                    }
                    else
                    {
                        float amount = Mathf.Min((float)statusEffects[i].parameter, (float)statusEffects[i].parameter / statusEffects[i].duration * overTimeEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            temp = reservedHealCasters.IndexOf(statusEffects[i].caster);
                            if (temp != -1)
                            {
                                reservedHealAmounts[temp] += amount;
                            }
                            else
                            {
                                reservedHealAmounts.Add(amount);
                                reservedHealCasters.Add(statusEffects[i].caster);
                            }

                            //ce.caster.DoHeal(amount, livingThing);
                        }
                        statusEffects[i].parameter = (float)statusEffects[i].parameter - amount;
                    }
                }
                else if (statusEffects[i].type == StatusEffectType.DamageOverTime)
                {
                    if (statusEffects[i].duration == 0)
                    {
                        if (photonView.IsMine)
                        {
                            temp = reservedMagicDamageCasters.IndexOf(statusEffects[i].caster);
                            if (temp != -1)
                            {
                                reservedMagicDamageAmounts[temp] += (float)statusEffects[i].parameter;
                            }
                            else
                            {
                                reservedMagicDamageAmounts.Add((float)statusEffects[i].parameter);
                                reservedMagicDamageCasters.Add(statusEffects[i].caster);
                            }

                            //ce.caster.DoMagicDamage((float)ce.parameter, livingThing, true);
                        }
                        removeList.Add(statusEffects[i]);
                    }
                    else
                    {
                        float amount = Mathf.Min((float)statusEffects[i].parameter, (float)statusEffects[i].parameter / statusEffects[i].duration * overTimeEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            temp = reservedMagicDamageCasters.IndexOf(statusEffects[i].caster);
                            if (temp != -1)
                            {
                                reservedMagicDamageAmounts[temp] += amount;
                            }
                            else
                            {
                                reservedMagicDamageAmounts.Add(amount);
                                reservedMagicDamageCasters.Add(statusEffects[i].caster);
                            }
                            //ce.caster.DoMagicDamage(amount, livingThing, true);
                        }
                        statusEffects[i].parameter = (float)statusEffects[i].parameter - amount;
                    }
                }
            }


            if (photonView.IsMine)
            {
                if ((statusEffects[i].type == StatusEffectType.Shield && (float)statusEffects[i].parameter <= 0) || (statusEffects[i].duration <= 0 && statusEffects[i].type != StatusEffectType.HealOverTime && statusEffects[i].type != StatusEffectType.DamageOverTime) || (!SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(livingThing) && statusEffects[i].IsHarmful()) || livingThing.IsDead())
                {
                    removeList.Add(statusEffects[i]);
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

        totalAttackDamageBoostAmount = 0;
        totalAttackDamageReductionAmount = 0;

        totalSpellPowerBoostAmount = 0;
        totalSpellPowerReductionAmount = 0;

        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == StatusEffectType.Haste && statusEffects[i].parameter != null)
            {
                totalHasteAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.Slow && statusEffects[i].parameter != null)
            {
                totalSlowAmount = Mathf.Max(totalSlowAmount, (float)statusEffects[i].parameter);
            }
            else if (statusEffects[i].type == StatusEffectType.Speed && statusEffects[i].parameter != null)
            {
                totalSpeedAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.HealOverTime)
            {
                totalHealOverTimeAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.DamageOverTime)
            {
                totalDamageOverTimeAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.Shield)
            {
                totalShieldAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.AttackDamageBoost)
            {
                totalAttackDamageBoostAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.AttackDamageReduction)
            {
                totalAttackDamageReductionAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.SpellPowerBoost)
            {
                totalSpellPowerBoostAmount += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.SpellPowerReduction)
            {
                totalSpellPowerReductionAmount += (float)statusEffects[i].parameter;
            }
        }

        for(int i = 0; i < removeList.Count; i++)
        {
            RemoveStatusEffect(removeList[i]);
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

    }

    public void ApplyShieldDamage(float amount)
    {
        photonView.RPC("RpcApplyShieldDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    public void RpcApplyShieldDamage(float amount)
    {
        float remainingAmount = amount;
        for (int i= 0;i < statusEffects.Count;i++)
        {
            if(statusEffects[i].type == StatusEffectType.Shield)
            {
                if((float)statusEffects[i].parameter >= remainingAmount)
                {
                    statusEffects[i].parameter = (float)statusEffects[i].parameter - remainingAmount;
                    break;
                }
                else
                {
                    remainingAmount -= (float)statusEffects[i].parameter;
                    statusEffects[i].parameter = 0f;
                }
            }
        }
    }


    [PunRPC]
    public void RpcApplyStatusEffect(long uid, int casterViewID, byte type, float duration, object parameter)
    {
        LivingThing caster = PhotonNetwork.GetPhotonView(casterViewID).GetComponent<LivingThing>();
        LivingThing owner = livingThing;

        StatusEffect ce = new StatusEffect(caster, (StatusEffectType)type, duration, parameter);
        ce.owner = livingThing;
        ce.uid = uid;
        statusEffects.Add(ce);
        statusEffectCountMap[(int)ce.type] += 1;
        if(statusEffectCountMap[(int)ce.type] == 1) StatusEffectVisualsManager.CreateVisual(livingThing, ce.type);
    }

    [PunRPC]
    public void RpcRemoveStatusEffect(long uid)
    {
        StatusEffect se = GetStatusEffectByUID(uid);
        if (se == null) return;
        statusEffectCountMap[(int)se.type] -= 1;
        se.duration = 0;
        se.OnExpire();
        statusEffects.Remove(se);
    }

    [PunRPC]
    public void RpcAddDurationToStatusEffect(long uid, float duration)
    {
        StatusEffect se = GetStatusEffectByUID(uid);
        if(se != null) se.duration += duration;
    }

    [PunRPC]
    public void RpcSetDurationOfStatusEffect(long uid, float duration)
    {
        StatusEffect se = GetStatusEffectByUID(uid);
        if (se != null) se.duration = duration;
    }

    [PunRPC]
    public void RpcSetParameterOfStatusEffect(long uid, object parameter)
    {
        StatusEffect se = GetStatusEffectByUID(uid);
        if (se != null) se.parameter = parameter;
    }


    [PunRPC]
    public void RpcCleanseStatusEffect(byte type)
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].type == (StatusEffectType)type)
            {
                statusEffects[i].duration = 0;
                statusEffects[i].OnExpire();
                statusEffects.RemoveAt(i);
                statusEffectCountMap[type] -= 1;
            }
        }
    }

    [PunRPC]
    public void RpcCleanseAllHarmfulStatusEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].IsHarmful())
            {
                statusEffectCountMap[(int)statusEffects[i].type] -= 1;
                statusEffects[i].duration = 0;
                statusEffects[i].OnExpire();
                statusEffects.RemoveAt(i);
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
