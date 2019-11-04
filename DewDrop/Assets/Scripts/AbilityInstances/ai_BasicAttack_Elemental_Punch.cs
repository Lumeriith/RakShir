using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_BasicAttack_Elemental_Punch : AbilityInstance
{

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;

        info.owner.DoBasicAttackImmediately(info.target);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
