using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Rare_Consume : AbilityInstance
{
    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.position = info.owner.transform.position;
        transform.parent = info.owner.transform;
        if (!isMine) return;
        info.owner.DoHeal((float)data[0], info.owner, true, source);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
