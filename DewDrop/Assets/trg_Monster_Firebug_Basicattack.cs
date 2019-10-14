using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Firebug_Basicattack : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, 0.5f, false, false, false, false, ChannelSuccess, null);
        info.owner.control.StartChanneling(channel, true);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_Firebug_Basicattack", info.owner.transform.position, Quaternion.identity, info);
    }
}
