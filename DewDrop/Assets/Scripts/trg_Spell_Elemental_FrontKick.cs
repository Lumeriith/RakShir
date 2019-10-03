using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Elemental_FrontKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.2f, false, false, false, false, ChannelSuccess, null);
        owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_FrontKick", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
    }
}
