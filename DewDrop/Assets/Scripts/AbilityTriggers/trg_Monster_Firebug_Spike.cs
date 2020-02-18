using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Firebug_Spike : AbilityTrigger
{
    
    public override void OnCast(CastInfo info)
    {
        StartCooldown();
        owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, ChannelFinished, null));
    }


    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Monster_Firebug_Spike", owner.transform.position, Quaternion.identity, info);
        owner.control.StartChanneling(new Channel(selfValidator, 0.3f, false, false, false, false, null, null));
    }
}
