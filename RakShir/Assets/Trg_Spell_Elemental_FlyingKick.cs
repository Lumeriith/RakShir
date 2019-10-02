using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trg_Spell_Elemental_FlyingKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.2f, false, false, true, false, ChannelSuccess, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_FlyingKick", transform.position, info.directionQuaternion, info);
    }

}
