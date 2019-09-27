using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_FlameShot : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        owner.control.StartChanneling(0.25f, ChannelSuccess);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_FlameShot", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
        StartCooldown();
    }
}
