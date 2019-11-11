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
        info.owner.ApplyStatusEffect(StatusEffect.Shield(info.owner,shieldDuration,shieldMultiplier * info.owner.stat.finalMaximumHealth, true));
        info.owner.ApplyStatusEffect(StatusEffect.Unstoppable(info.owner, unstoppableDuration));
        DestroySelf(5f);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
