using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trg_Spell_Default_FlyingKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        info.owner.control.StartChanneling(0.2f, ChannelSuccess);
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Default_FlyingKick", transform.position, info.directionQuaternion, info);
    }

}
