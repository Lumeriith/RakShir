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
    #region Instance Members

    public int uid;
    public LivingThing caster;
    public LivingThing owner;
    public StatusEffectType type;
    public float duration;
    public float originalDuration;
    public object parameter;

    public bool isAlive
    {
        get
        {
            return (duration > 0) && owner.statusEffect.GetStatusEffectByUID(uid) != null;
        }
    }

    public StatusEffect(LivingThing caster, StatusEffectType type, float duration, object parameter = null)
    {
        this.caster = caster;
        this.type = type;
        this.duration = duration;
        this.parameter = parameter;
        this.originalDuration = duration;
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

    public void ResetDuration()
    {
        owner.statusEffect.SetDurationOfStatusEffect(this, originalDuration);
    }


    public bool IsHarmful()
    {
        return type >= StatusEffectType.Stun && type <= StatusEffectType.DamageOverTime;
    }

    public bool IsBeneficial()
    {
        return type >= StatusEffectType.Invulnerable && type <= StatusEffectType.HealOverTime;
    }

    #endregion Instance Members

    #region Static Members

    public static StatusEffect Stasis(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Stasis, duration);
    }
    public static StatusEffect Invulnerable(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Invulnerable, duration);
    }
    public static StatusEffect Untargetable(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Untargetable, duration);
    }
    public static StatusEffect Unstoppable(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Unstoppable, duration);
    }
    public static StatusEffect Protected(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Protected, duration);
    }
    public static StatusEffect Speed(LivingThing caster, float duration, float amount)
    {
        return new StatusEffect(caster, StatusEffectType.Speed, duration, amount);
    }
    public static StatusEffect Haste(LivingThing caster, float duration, float amount)
    {
        return new StatusEffect(caster, StatusEffectType.Haste, duration, amount);
    }
    public static StatusEffect HealOverTime(LivingThing caster, float duration, float amount)
    {
        return new StatusEffect(caster, StatusEffectType.HealOverTime, duration, amount);
    }
    public static StatusEffect Stun(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Stun, duration);
    }
    public static StatusEffect Root(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Root, duration);
    }
    public static StatusEffect Slow(LivingThing caster, float duration, float amount)
    {
        return new StatusEffect(caster, StatusEffectType.Slow, duration, amount);
    }
    public static StatusEffect Silence(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Silence, duration);
    }
    public static StatusEffect DamageOverTime(LivingThing caster, float duration, float amount)
    {
        return new StatusEffect(caster, StatusEffectType.DamageOverTime, duration, amount);
    }
    public static StatusEffect Custom(LivingThing caster, string name, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Custom, duration, name);
    }

    #endregion Static Members

}

