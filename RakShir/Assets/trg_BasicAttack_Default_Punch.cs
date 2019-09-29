using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Default_Punch : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        owner.control.StartBasicAttackChanneling(2f / 5f, ChannelSuccess, ResetCooldown);
        StartBasicAttackCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Default_Punch", info.target.GetRandomOffset() + info.target.transform.position, Quaternion.identity, info);
    }

    
}
