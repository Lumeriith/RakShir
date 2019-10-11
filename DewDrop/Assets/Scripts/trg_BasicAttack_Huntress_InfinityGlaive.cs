using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Huntress_InfinityGlaive : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.3f, false, true, false, true, ChannelSuccess, ResetCooldown);
        info.owner.control.StartChanneling(channel, true);
        StartCooldown(true);
    }


    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Huntress_InfinityGlaive", info.owner.rightHand.position, Quaternion.identity, info);
    }
}
