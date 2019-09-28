using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_SmallFireball : AbilityTrigger
{
    private CastInfo info;
    
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        owner.control.StartBasicAttackChanneling(0.10f, ChannelSuccess, ResetCooldown);
        StartBasicAttackCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_SmallFireball", owner.rightHand.position, Quaternion.identity, info);
        owner.control.ReserveAbilityTrigger(this, target: info.target);

    }

}
