using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_SmallFireball : AbilityTrigger
{
    
    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, 1f / 3f, false, false, false, true, ChannelSuccess, ResetCooldown, true);
        owner.control.StartChanneling(channel);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_BasicAttack_SmallFireball", owner.rightHand.position, Quaternion.identity, info);
    }

}
