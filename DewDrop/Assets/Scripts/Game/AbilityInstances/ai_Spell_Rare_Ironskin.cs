using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Ironskin : AbilityInstance
{
    public float shieldMultiplier = 0.30f;
    public float shieldDuration = 6f;
    public float unstoppableDuration = 1.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info.owner.ApplyStatusEffect(StatusEffect.Shield(shieldDuration, shieldMultiplier * info.owner.stat.finalMaximumHealth, true), reference);
        info.owner.ApplyStatusEffect(StatusEffect.Unstoppable(unstoppableDuration), reference);
        Despawn(info.owner);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
