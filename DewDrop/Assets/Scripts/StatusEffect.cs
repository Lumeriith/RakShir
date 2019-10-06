using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StatusEffectType : byte
{
    // Neutral
    Stasis, Dash,

    // Beneficial
    Invulnerable, Untargetable, Unstoppable, Protected, Speed, Haste, HealOverTime,

    // Harmful
    Stun, Airborne, Sleep,

    Polymorph,

    MindControl, Charm, Fear,

    Root, Slow,

    Silence, Blind,

    DamageOverTime,

    // Custom
    Custom
}
public class StatusEffect
{
    public int uid;
    public LivingThing caster;
    public LivingThing owner;
    public StatusEffectType type;
    public float duration;
    public object parameter;
    public bool isAboutToBeDestroyed;

    public bool isAlive
    {
        get
        {
            return !isAboutToBeDestroyed && !(duration <= 0) && owner.statusEffect.RetrieveStatusEffect(uid) != null;
        }
    }

    public StatusEffect(LivingThing caster, StatusEffectType type, float duration, object parameter = null)
    {
        this.caster = caster;
        this.type = type;
        this.duration = duration;
        this.parameter = parameter;
        this.isAboutToBeDestroyed = false;
    }

    public void Remove()
    {
        owner.statusEffect.RemoveStatusEffect(this);
    }

    public void AddDuration(float duration)
    {
        owner.statusEffect.AddDurationToStatusEffect(this, duration);
    }

    public void SetDuration(float duration)
    {
        owner.statusEffect.SetDurationOfStatusEffect(this, duration);
    }


    public bool IsHarmful()
    {
        return type >= StatusEffectType.Stun && type <= StatusEffectType.DamageOverTime;
    }

    public bool IsBeneficial()
    {
        return type >= StatusEffectType.Invulnerable && type <= StatusEffectType.HealOverTime;
    }
}

