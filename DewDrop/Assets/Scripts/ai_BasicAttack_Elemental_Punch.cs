using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_BasicAttack_Elemental_Punch : AbilityInstance
{
    CastInfo info;
    public float slowDuration = 0.25f;
    public float slowAmount = 50f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info = castInfo;

        StatusEffect slow = new StatusEffect(info.owner, StatusEffectType.Slow, slowDuration, slowAmount);
        castInfo.target.statusEffect.ApplyStatusEffect(slow);

        info.owner.DoBasicAttackImmediately(info.target);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
