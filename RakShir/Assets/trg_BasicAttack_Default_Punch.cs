using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Default_Punch : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.23f, false, false, false, true, ChannelSuccess, ResetCooldown);
        owner.control.StartChanneling(channel, true);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Default_Punch", info.target.GetRandomOffset() + info.target.transform.position, Quaternion.identity, info);
    }

    
}
