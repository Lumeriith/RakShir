using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Rhino_Basicattack : AbilityInstance
{
    public float chargeDistance = 1.5f;
    public float duration = 0.5f;
    public float slowDuration = 0.65f;
    public float slowAmount = 40f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.Find("Flash").position = castInfo.target.transform.position + castInfo.target.GetCenterOffset();
        if (!photonView.IsMine) return;

        info.owner.StartDisplacement(Displacement.ByVector((info.target.transform.position - info.owner.transform.position).normalized * chargeDistance, duration, true, true, false));
        info.target.ApplyStatusEffect(StatusEffect.Slow(source, slowDuration, slowAmount));
        castInfo.owner.DoBasicAttackImmediately(castInfo.target, source);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }

    
}
