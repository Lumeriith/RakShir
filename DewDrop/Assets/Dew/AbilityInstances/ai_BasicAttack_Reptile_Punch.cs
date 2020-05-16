using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_BasicAttack_Reptile_Punch : AbilityInstance
{

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        SFXManager.CreateSFXInstance("si_BasicAttack_Reptile_Punch", info.target.transform.position);
        info.owner.DoBasicAttackImmediately(info.target, this);
        Despawn();
    }
}
