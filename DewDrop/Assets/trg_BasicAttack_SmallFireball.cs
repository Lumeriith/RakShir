using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_SmallFireball : AbilityTrigger
{
    private CastInfo info;
    
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 1f / 3f, false, false, false, true, ChannelSuccess, ResetCooldown);
        owner.control.StartChanneling(channel, true);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_SmallFireball", owner.rightHand.position, Quaternion.identity, info);
    }

}
