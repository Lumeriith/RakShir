using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Huntress_InfinityGlaive : AbilityTrigger
{

    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, 0.3f, false, false, false, true, ChannelSuccess, ResetCooldown, true);
        info.owner.control.StartChanneling(channel);
        StartCooldown(true);
    }


    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Huntress_InfinityGlaive", info.owner.rightHand.position, Quaternion.identity, info);
    }

}
