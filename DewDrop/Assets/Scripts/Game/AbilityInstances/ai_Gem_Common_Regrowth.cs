using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Common_Regrowth : AbilityInstance
{
    protected override void OnCreate(CastInfo info, object[] data)
    {
        Transform glow = transform.Find("Glow");
        glow.parent = info.owner.transform;
        glow.gameObject.AddComponent<ParticleSystemAutoDestroy>();
        if (!isMine) return;
        info.owner.DoHeal(((gem_Common_Regrowth)source.gem).healAmount[source.gem.level], info.owner, false, source);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
