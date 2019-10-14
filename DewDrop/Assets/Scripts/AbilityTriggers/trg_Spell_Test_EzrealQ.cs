using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Test_EzrealQ : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.5f, false, false, false, false, SuccessCallback, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void SuccessCallback()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Test_EzrealQ", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info);
    }


}
