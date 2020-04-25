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

    Shield,

    SpellPowerBoost, SpellPowerReduction,

    AttackDamageBoost, AttackDamageReduction
}

[System.Serializable]
public class StatusEffect
{
    #region Instance Members

    public long uid;
    public IDewActionCaller handler;
    public LivingThing owner;
    public StatusEffectType type;
    public float duration;
    public float originalDuration;
    public object parameter;

    public System.Action OnExpire = () => { };

    public bool isAlive
    {
        get
        {
            return (duration > 0) && owner != null && owner.statusEffect.GetStatusEffectByUID(uid) != null;
        }
    }

    public StatusEffect(StatusEffectType type, float duration, object parameter = null)
    {
        this.type = type;
        this.duration = duration;
        this.parameter = parameter;
        this.originalDuration = duration;
    }

    public void SetHandler(IDewActionCaller caller)
    {
        handler = caller;
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

    private static StatusEffectType[] statusEffectsToDisplay =
{
        StatusEffectType.Stasis,
        StatusEffectType.Invulnerable,
        StatusEffectType.Protected,
        StatusEffectType.Untargetable,
        StatusEffectType.Unstoppable,
        StatusEffectType.MindControl,
        StatusEffectType.Polymorph,
        StatusEffectType.Stun,
        StatusEffectType.Sleep,
        StatusEffectType.Charm,
        StatusEffectType.Fear,
        StatusEffectType.Silence,
        StatusEffectType.Root,
        StatusEffectType.Blind,
        StatusEffectType.Custom
    };

    private static string[] statusEffectNamesToDisplay =
    {
        "정지", "무적", "보호", "지정불가", "저지불가", "정신조종", "변이", "기절", "수면", "매혹", "공포", "침묵", "이동불가", "실명", ""
    };

    public static string GetImportantStatusEffectName(LivingThing thing)
    {
        List<StatusEffectType> existingTypes = new List<StatusEffectType>();
        for (int i = 0; i < thing.statusEffect.statusEffects.Count; i++)
        {
            if (!existingTypes.Contains(thing.statusEffect.statusEffects[i].type)) existingTypes.Add(thing.statusEffect.statusEffects[i].type);
        }
        if (existingTypes.Count == 0) return "";
        for (int i = 0; i < statusEffectsToDisplay.Length; i++)
        {
            if (existingTypes.Contains(statusEffectsToDisplay[i]))
            {
                if (statusEffectsToDisplay[i] == StatusEffectType.Custom) return (string)thing.statusEffect.statusEffects.Find(x => x.type == StatusEffectType.Custom).parameter;
                return statusEffectNamesToDisplay[i];
            }
        }
        return "";
    }

    public static string GetImportantStatusEffectNames(LivingThing thing)
    {
        string result = "";
        List<StatusEffectType> existingTypes = new List<StatusEffectType>();
        for (int i = 0; i < thing.statusEffect.statusEffects.Count; i++)
        {
            if (!existingTypes.Contains(thing.statusEffect.statusEffects[i].type)) existingTypes.Add(thing.statusEffect.statusEffects[i].type);
        }
        if (existingTypes.Count == 0) return "";
        for (int i = 0; i < statusEffectsToDisplay.Length; i++)
        {
            if (existingTypes.Contains(statusEffectsToDisplay[i]))
            {
                if (statusEffectsToDisplay[i] == StatusEffectType.Custom) result += (string)thing.statusEffect.statusEffects.Find(x => x.type == StatusEffectType.Custom).parameter + " ";
                else result += statusEffectNamesToDisplay[i] + " ";
            }
        }
        if (result.EndsWith(" ")) result = result.Substring(0, result.Length - 1);
        return result;
    }


    public static StatusEffect Stasis(float duration)
    {
        return new StatusEffect(StatusEffectType.Stasis, duration);
    }
    public static StatusEffect Invulnerable(float duration)
    {
        return new StatusEffect(StatusEffectType.Invulnerable, duration);
    }
    public static StatusEffect Untargetable(float duration)
    {
        return new StatusEffect(StatusEffectType.Untargetable, duration);
    }
    public static StatusEffect Unstoppable(float duration)
    {
        return new StatusEffect(StatusEffectType.Unstoppable, duration);
    }
    public static StatusEffect Protected(float duration)
    {
        return new StatusEffect(StatusEffectType.Protected, duration);
    }
    public static StatusEffect Speed(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.Speed, duration, amount);
    }
    public static StatusEffect Haste(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.Haste, duration, amount);
    }
    public static StatusEffect HealOverTime(float duration, float amount, bool ignoreSpellPower = false)
    {
        // TODO spellpower is broken
        return new StatusEffect(StatusEffectType.HealOverTime, duration, amount);
    }
    public static StatusEffect Stun(float duration)
    {
        return new StatusEffect(StatusEffectType.Stun, duration);
    }
    public static StatusEffect Root(float duration)
    {
        return new StatusEffect(StatusEffectType.Root, duration);
    }
    public static StatusEffect Slow(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.Slow, duration, amount);
    }
    public static StatusEffect Silence(float duration)
    {
        return new StatusEffect(StatusEffectType.Silence, duration);
    }

    public static StatusEffect Blind(float duration)
    {
        return new StatusEffect(StatusEffectType.Blind, duration);
    }


    public static StatusEffect DamageOverTime(float duration, float amount, bool ignoreSpellPower = false)
    {
        // TODO spellpower is broken
        // return new StatusEffect(StatusEffectType.DamageOverTime, duration, (ignoreSpellPower || handler.invokerEntity == null) ? amount : amount * handler.invokerEntity.stat.finalSpellPower / 100f);
        return new StatusEffect(StatusEffectType.DamageOverTime, duration, amount);
    }
    public static StatusEffect Custom(string name, float duration)
    {
        return new StatusEffect(StatusEffectType.Custom, duration, name);
    }

    public static StatusEffect Shield(float duration, float amount, bool ignoreSpellPower = false)
    {
        // TODO spellpower is broken
        return new StatusEffect(StatusEffectType.Shield, duration, amount);
        // return new StatusEffect(StatusEffectType.Shield, duration, (ignoreSpellPower || handler.invokerEntity == null) ? amount : amount * handler.invokerEntity.stat.finalSpellPower / 100f);
    }

    public static StatusEffect AttackDamageBoost(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.AttackDamageBoost, duration, amount);
    }

    public static StatusEffect AttackDamageReduction(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.AttackDamageReduction, duration, amount);
    }

    public static StatusEffect SpellPowerBoost(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.SpellPowerBoost, duration, amount);
    }

    public static StatusEffect SpellPowerReduction(float duration, float amount)
    {
        return new StatusEffect(StatusEffectType.SpellPowerReduction, duration, amount);
    }



    #endregion Static Members

}

