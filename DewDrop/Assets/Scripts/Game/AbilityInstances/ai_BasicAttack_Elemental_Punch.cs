using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_BasicAttack_Elemental_Punch : AbilityInstance
{

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        SFXManager.CreateSFXInstance("si_BasicAttack_Elemental_Punch " + Random.Range(0, 2), info.target.transform.position);
        info.owner.DoBasicAttackImmediately(info.target, source);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
