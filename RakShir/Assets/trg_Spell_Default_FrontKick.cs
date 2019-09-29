using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Default_FrontKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        owner.control.StartChanneling(0.2f, ChannelSuccess);
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Default_FrontKick", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
    }
}
