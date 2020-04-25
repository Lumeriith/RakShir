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
        info.owner.DoHeal(info.owner, ((gem_Common_Regrowth)gem).healAmount[gem.level], false, reference);
        
        Despawn();
    }
}
