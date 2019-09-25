using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CoreStatusEffectType
{
    // Neutral
    Stasis,

    // Beneficial
    Invulnerable, Untargetable, Protected, Speed, Haste,

    // Harmful
    Stun, Airborne, Sleep,

    Polymorph,

    Charm, Fear,

    Root, Slow,

    Silence, Blind
}
public class CoreStatusEffect
{
    public int uid;
    public LivingThing caster;
    public CoreStatusEffectType type;
    public float duration;
    public object parameter;

    CoreStatusEffect(LivingThing caster, CoreStatusEffectType type, float duration, object parameter = null)
    {
        this.caster = caster;
        this.type = type;
        this.duration = duration;
        this.parameter = null;
    }

    public bool IsHarmful()
    {
        return type >= CoreStatusEffectType.Stun && type <= CoreStatusEffectType.Blind;
    }

    public bool IsBeneficial()
    {
        return type >= CoreStatusEffectType.Invulnerable && type <= CoreStatusEffectType.Haste;
    }
}

