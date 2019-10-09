using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_FlameShot : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.35f, false, false, false, false, ChannelSuccess, null);
        owner.control.StartChanneling(channel);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_FlameShot", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
        StartCooldown();
    }
}
