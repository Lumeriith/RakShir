using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Default_FrontKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, false, 0.2f, ChannelSuccess, null, CommandLockType.DisallowAll);
        owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Default_FrontKick", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
    }
}
