using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Gem_Epic_Time : AbilityInstance
{
    private gem_Epic_Time time;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
        if (!isMine) return;
        time = (gem_Epic_Time)source.gem;
        time.trigger.ApplyCooldownReduction(time.cooldownReduction[time.level]);
        SFXManager.CreateSFXInstance("si_Gem_Epic_Time", transform.position);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }


}
