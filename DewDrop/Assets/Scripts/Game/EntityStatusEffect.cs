using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public interface IReadOnlyEntityStatus
{
    float speed { get; }
    float haste { get; }
    float slow { get; }

    float healOverTime { get; }
    float damageOverTime { get; }
    float shield { get; }

    float spellPowerReduction { get; }
    float spellPowerBoost { get; }
    float attackDamageReduction { get; }
    float attackDamageBoost { get; }
}

public struct EntityStatus : IReadOnlyEntityStatus
{
    public float speed { get; set; }
    public float haste { get; set; }
    public float slow { get; set; }

    public float healOverTime { get; set; }
    public float damageOverTime { get; set; }
    public float shield { get; set; }

    public float spellPowerReduction { get; set; }
    public float spellPowerBoost { get; set; }
    public float attackDamageReduction { get; set; }
    public float attackDamageBoost { get; set; }

    public void Clear()
    {
        speed = 0f;
        haste = 0f;
        slow = 0f;
        healOverTime = 0f;
        damageOverTime = 0f;
        shield = 0f;
        spellPowerReduction = 0f;
        spellPowerBoost = 0f;
        attackDamageReduction = 0f;
        attackDamageBoost = 0f;
    }
}


public class EntityStatusEffect : MonoBehaviourPun
{
    private const float OverTimeStatusEffectTickInterval = 0.25f;

    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    private Entity _entity;

    private float _lastOverTimeStatusEffectTickTime = 0f;

    private int[] _statusEffectCountMap;

    public IReadOnlyEntityStatus status { get => _status; }
    private EntityStatus _status;

    private void Awake()
    {
        int maxStatusEffectTypeValue = 0;
        foreach(StatusEffectType type in System.Enum.GetValues(typeof(StatusEffectType)))
        {
            if ((int)type > maxStatusEffectTypeValue) maxStatusEffectTypeValue = (int)type;
        }
        _statusEffectCountMap = new int[maxStatusEffectTypeValue + 1];
        _entity = GetComponent<Entity>();
        _lastOverTimeStatusEffectTickTime = Time.time;
        
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

    public void ApplyStatusEffect(StatusEffect ce, DewActionCaller caller)
    {
        if (ce.IsHarmful() && !SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(_entity)) return;
        long uid; 
        do
        {
            uid = PhotonNetwork.LocalPlayer.ActorNumber + Random.Range(int.MinValue, int.MaxValue) << 32; // thank you ggsg ✨
            // it's probably okay to make guids sequentially generated, but better check the code thoroughly before doing it.
        } while (GetStatusEffectByUID(uid) != null);

        ce.uid = uid;
        ce.owner = _entity;
        ce.handler = caller;
        statusEffects.Add(ce);
        _statusEffectCountMap[(int)ce.type] += 1;
        if(_statusEffectCountMap[(int)ce.type] == 1) StatusEffectVisualsManager.CreateVisual(_entity, ce.type);
        photonView.RPC(nameof(RpcApplyStatusEffect), RpcTarget.Others, uid, ce.handler?.GetActionCallerUID(), (byte)ce.type, ce.duration, ce.parameter);
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
        if(type == StatusEffectType.Dash) return _entity.ongoingDisplacement != null && _entity.ongoingDisplacement.isFriendly;
        if (type == StatusEffectType.Airborne) return _entity.ongoingDisplacement != null && !_entity.ongoingDisplacement.isFriendly;
        return _statusEffectCountMap[(int)type] > 0;
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
        bool canTick = SelfValidator.CanTick.Evaluate(_entity);
        bool tickedAirborne = false;
        bool doOverTimeEffectTicks = false;
        float remainingAirboneDuration = 0;

        List<StatusEffect> removeList = new List<StatusEffect>();




        if (Time.time - _lastOverTimeStatusEffectTickTime > OverTimeStatusEffectTickInterval)
        {
            _lastOverTimeStatusEffectTickTime += OverTimeStatusEffectTickInterval;
            doOverTimeEffectTicks = true;
            _lastOverTimeStatusEffectTickTime = Time.time;
        }

        List<float> reservedHealAmounts = new List<float>();
        List<DewActionCaller> reservedHealHandlers = new List<DewActionCaller>();

        List<float> reservedMagicDamageAmounts = new List<float>();
        List<DewActionCaller> reservedMagicDamageHandlers = new List<DewActionCaller>();

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
                            temp = -1;
                            for(int j = 0; j < reservedHealAmounts.Count; j++)
                            {
                                if(reservedHealHandlers[j] == statusEffects[i].handler)
                                {
                                    temp = j;
                                    break;
                                }
                            }

                            if(temp != -1)
                            {
                                reservedHealAmounts[temp] += (float)statusEffects[i].parameter;
                            }
                            else
                            {
                                reservedHealAmounts.Add((float)statusEffects[i].parameter);
                                reservedHealHandlers.Add(statusEffects[i].handler);
                            }

                            //ce.caster.DoHeal((float)ce.parameter, livingThing, true);
                            removeList.Add(statusEffects[i]);
                        }
                    }
                    else
                    {
                        float amount = Mathf.Min((float)statusEffects[i].parameter, (float)statusEffects[i].parameter / statusEffects[i].duration * OverTimeStatusEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            temp = -1;
                            for (int j = 0; j < reservedHealAmounts.Count; j++)
                            {
                                if (reservedHealHandlers[j] == statusEffects[i].handler)
                                {
                                    temp = j;
                                    break;
                                }
                            }

                            if (temp != -1)
                            {
                                reservedHealAmounts[temp] += amount;
                            }
                            else
                            {
                                reservedHealAmounts.Add(amount);
                                reservedHealHandlers.Add(statusEffects[i].handler);
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
                            temp = -1;
                            for (int j = 0; j < reservedMagicDamageAmounts.Count; j++)
                            {
                                if (reservedMagicDamageHandlers[j] == statusEffects[i].handler)
                                {
                                    temp = j;
                                    break;
                                }
                            }

                            if (temp != -1)
                            {
                                reservedMagicDamageAmounts[temp] += (float)statusEffects[i].parameter;
                            }
                            else
                            {
                                reservedMagicDamageAmounts.Add((float)statusEffects[i].parameter);
                                reservedMagicDamageHandlers.Add(statusEffects[i].handler);
                            }

                            //ce.caster.DoMagicDamage((float)ce.parameter, livingThing, true);
                        }
                        removeList.Add(statusEffects[i]);
                    }
                    else
                    {
                        float amount = Mathf.Min((float)statusEffects[i].parameter, (float)statusEffects[i].parameter / statusEffects[i].duration * OverTimeStatusEffectTickInterval);
                        if (photonView.IsMine)
                        {
                            temp = -1;
                            for (int j = 0; j < reservedMagicDamageAmounts.Count; j++)
                            {
                                if (reservedMagicDamageHandlers[j] == statusEffects[i].handler)
                                {
                                    temp = j;
                                    break;
                                }
                            }

                            if (temp != -1)
                            {
                                reservedMagicDamageAmounts[temp] += amount;
                            }
                            else
                            {
                                reservedMagicDamageAmounts.Add(amount);
                                reservedMagicDamageHandlers.Add(statusEffects[i].handler);
                            }
                            //ce.caster.DoMagicDamage(amount, livingThing, true);
                        }
                        statusEffects[i].parameter = (float)statusEffects[i].parameter - amount;
                    }
                }
            }


            if (photonView.IsMine)
            {
                if ((statusEffects[i].type == StatusEffectType.Shield && (float)statusEffects[i].parameter <= 0) || (statusEffects[i].duration <= 0 && statusEffects[i].type != StatusEffectType.HealOverTime && statusEffects[i].type != StatusEffectType.DamageOverTime) || (!SelfValidator.CanHaveHarmfulStatusEffects.Evaluate(_entity) && statusEffects[i].IsHarmful()) || _entity.IsDead())
                {
                    removeList.Add(statusEffects[i]);
                }
            }

        }

        for (int i = 0; i < reservedHealAmounts.Count; i++)
        {
            if(reservedHealHandlers[i] == null || reservedHealHandlers[i].entity == null) _entity.DoHeal(_entity, reservedHealAmounts[i], true, reservedHealHandlers[i]);
            else reservedHealHandlers[i].entity.DoHeal(_entity, reservedHealAmounts[i], true, reservedHealHandlers[i]);

        }

        for (int i = 0; i < reservedMagicDamageAmounts.Count; i++)
        {
            if(reservedMagicDamageHandlers[i] == null || reservedMagicDamageHandlers[i].entity == null) _entity.DoMagicDamage(_entity, reservedMagicDamageAmounts[i], true, reservedMagicDamageHandlers[i]);
            else reservedMagicDamageHandlers[i].entity.DoMagicDamage(_entity, reservedMagicDamageAmounts[i], true, reservedMagicDamageHandlers[i]);
        }

        _status.Clear();

        for(int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == StatusEffectType.Haste && statusEffects[i].parameter != null)
            {
                _status.haste += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.Slow && statusEffects[i].parameter != null)
            {
                _status.slow = Mathf.Max(_status.slow, (float)statusEffects[i].parameter);
            }
            else if (statusEffects[i].type == StatusEffectType.Speed && statusEffects[i].parameter != null)
            {
                _status.speed += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.HealOverTime)
            {
                _status.healOverTime += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.DamageOverTime)
            {
                _status.damageOverTime += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.Shield)
            {
                _status.shield += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.AttackDamageBoost)
            {
                _status.attackDamageBoost += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.AttackDamageReduction)
            {
                _status.attackDamageReduction += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.SpellPowerBoost)
            {
                _status.spellPowerBoost += (float)statusEffects[i].parameter;
            }
            else if (statusEffects[i].type == StatusEffectType.SpellPowerReduction)
            {
                _status.spellPowerReduction += (float)statusEffects[i].parameter;
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
    public void RpcApplyStatusEffect(long uid, int callerUID, byte type, float duration, object parameter)
    {
        StatusEffect ce = new StatusEffect((StatusEffectType)type, duration, parameter);
        ce.owner = _entity;
        ce.uid = uid;
        ce.handler = DewActionCaller.Retrieve(callerUID);
        statusEffects.Add(ce);
        _statusEffectCountMap[(int)ce.type] += 1;
        if(_statusEffectCountMap[(int)ce.type] == 1) StatusEffectVisualsManager.CreateVisual(_entity, ce.type);
    }

    [PunRPC]
    public void RpcRemoveStatusEffect(long uid)
    {
        StatusEffect se = GetStatusEffectByUID(uid);
        if (se == null) return;
        _statusEffectCountMap[(int)se.type] -= 1;
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
                _statusEffectCountMap[type] -= 1;
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
                _statusEffectCountMap[(int)statusEffects[i].type] -= 1;
                statusEffects[i].duration = 0;
                statusEffects[i].OnExpire();
                statusEffects.RemoveAt(i);
            }
        }
    }

    [PunRPC]
    private void RpcStartStunned()
    {
        _entity.OnStartStunned.Invoke();
    }

    [PunRPC]
    private void RpcStopStunned()
    {
        _entity.OnStopStunned.Invoke();
    }
}
