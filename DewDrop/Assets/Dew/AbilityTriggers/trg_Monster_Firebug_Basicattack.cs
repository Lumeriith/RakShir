using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Firebug_Basicattack : AbilityTrigger
{
    public float channelDurationRatio = 0.5f;
    public float channelAfterDelayRatio = 0f;
    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, channelDurationRatio, false, false, false, false, ChannelSuccess, null, true);
        info.owner.control.StartChanneling(channel);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_Monster_Firebug_Basicattack", info.owner.transform.position, Quaternion.identity, info);
        Channel channel = new Channel(selfValidator, channelDurationRatio, false, false, false, false, null, null, true);
        info.owner.control.StartChanneling(channel);
    }
}
