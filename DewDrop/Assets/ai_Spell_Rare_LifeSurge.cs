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
        info.owner.ApplyStatusEffect(StatusEffect.HealOverTime(info.owner, healOverTimeDuration, healMultiplier * info.owner.stat.finalMaximumHealth, true));
        info.owner.DoHeal(healMultiplier * info.owner.stat.finalMaximumHealth, info.owner, true);
        info.owner.statusEffect.CleanseAllHarmfulStatusEffects();
        DestroySelf(5f);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
