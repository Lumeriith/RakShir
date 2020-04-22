using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Common_Frost : AbilityInstance
{
    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.position = info.target.transform.position + info.target.GetCenterOffset();
        transform.parent = info.target.transform;
        if (!isMine) return;
        gem_Common_Frost frost = (gem_Common_Frost)source.gem;
        info.target.ApplyStatusEffect(StatusEffect.Slow(source, frost.slowDuration[frost.level], frost.slowAmount[frost.level]));
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
