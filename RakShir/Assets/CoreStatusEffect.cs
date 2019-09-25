using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CoreStatusEffectType : byte
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
    public LivingThing owner;
    public CoreStatusEffectType type;
    public float duration;
    public object parameter;



    public CoreStatusEffect(LivingThing caster, LivingThing owner, CoreStatusEffectType type, float duration, object parameter)
    {
        this.caster = caster;
        this.owner = owner;
        this.type = type;
        this.duration = duration;
        this.parameter = parameter;
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

