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
    Custom,

    Shield
}

[System.Serializable]
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

    public void SetParameter(object parameter)
    {
        owner.statusEffect.SetParameterOfStatusEffect(this, parameter);
    }

    public void ResetDuration()
    {
        owner.statusEffect.SetDurationOfStatusEffect(this, originalDuration);
    }


    public bool IsHarmful()
    {
        return harmfulEffectTypes.Contains(type);
    }


    #endregion Instance Members

    #region Static Members

    private static List<StatusEffectType> harmfulEffectTypes = new List<StatusEffectType>
    {
        StatusEffectType.Stun,
        StatusEffectType.Airborne,
        StatusEffectType.Sleep,
        StatusEffectType.Polymorph,
        StatusEffectType.MindControl,
        StatusEffectType.Charm,
        StatusEffectType.Fear,
        StatusEffectType.Root,
        StatusEffectType.Slow,
        StatusEffectType.Silence,
        StatusEffectType.Blind,
        StatusEffectType.DamageOverTime
    };
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
    public static StatusEffect HealOverTime(LivingThing caster, float duration, float amount, bool ignoreSpellPower = false)
    {
        return new StatusEffect(caster, StatusEffectType.HealOverTime, duration, ignoreSpellPower ? amount : amount * caster.stat.finalSpellPower / 100f);
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

    public static StatusEffect Blind(LivingThing caster, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Blind, duration);
    }


    public static StatusEffect DamageOverTime(LivingThing caster, float duration, float amount, bool ignoreSpellPower = false)
    {
        return new StatusEffect(caster, StatusEffectType.DamageOverTime, duration, ignoreSpellPower ? amount : amount * caster.stat.finalSpellPower / 100f);
    }
    public static StatusEffect Custom(LivingThing caster, string name, float duration)
    {
        return new StatusEffect(caster, StatusEffectType.Custom, duration, name);
    }

    public static StatusEffect Shield(LivingThing caster, float duration, float amount, bool ignoreSpellPower = false)
    {
        return new StatusEffect(caster, StatusEffectType.Shield, duration, ignoreSpellPower ? amount : amount * caster.stat.finalSpellPower / 100f);
    }

    #endregion Static Members

}

