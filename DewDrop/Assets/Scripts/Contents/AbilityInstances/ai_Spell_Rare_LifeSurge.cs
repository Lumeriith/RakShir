using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_LifeSurge : AbilityInstance
{
    public float healMultiplier = 0.25f;
    public float healOverTimeDuration = 5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info.owner.ApplyStatusEffect(StatusEffect.HealOverTime(healOverTimeDuration, healMultiplier * info.owner.stat.finalMaximumHealth, true), this);
        info.owner.DoHeal(info.owner, healMultiplier * info.owner.stat.finalMaximumHealth, true, this);
        info.owner.statusEffect.CleanseAllHarmfulStatusEffects();
        Despawn(info.owner);
    }
}
