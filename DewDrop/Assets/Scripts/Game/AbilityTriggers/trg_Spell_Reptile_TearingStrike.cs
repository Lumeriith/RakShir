using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Reptile_TearingStrike : AbilityTrigger
{
    public float channelTime = 0.1f;

    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, channelTime, false, false, false, false, ChannelSuccess, null);
        owner.control.StartChanneling(channel);
        StartCooldown();
        SpendMana();
    }

    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_Spell_Reptile_TearingStrike", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
    }
}
