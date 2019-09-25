using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CoreStatusEffectType : byte
{
    // Neutral
    Stasis,

    // Beneficial
    Invulnerable, Untargetable, Unstoppable, Protected, Speed, Haste,

    // Harmful
    Stun, Airborne, Sleep,

    Polymorph,

    MindControl, Charm, Fear,

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
    public bool isAboutToBeDestroyed;

    public bool isAlive
    {
        get
        {
            return !isAboutToBeDestroyed && !(duration <= 0) && owner.statusEffect.RetrieveCoreStatusEffect(uid) != null;
        }
    }

    public CoreStatusEffect(LivingThing caster, CoreStatusEffectType type, float duration, object parameter = null)
    {
        this.caster = caster;
        this.type = type;
        this.duration = duration;
        this.parameter = parameter;
        this.isAboutToBeDestroyed = false;
    }

    public void Remove()
    {
        owner.statusEffect.RemoveCoreStatusEffect(this);
    }

    public void AddDuration(float duration)
    {
        owner.statusEffect.AddDurationToCoreStatusEffect(this, duration);
    }

    public void SetDuration(float duration)
    {
        owner.statusEffect.SetDurationOfCoreStatusEffect(this, duration);
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

