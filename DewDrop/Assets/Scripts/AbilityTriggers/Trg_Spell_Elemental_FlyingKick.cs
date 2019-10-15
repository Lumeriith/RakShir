using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trg_Spell_Elemental_FlyingKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.1f, false, false, false, false, ChannelSuccess, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
        SpendMana();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_FlyingKick", owner.transform.position, info.directionQuaternion, info);
    }

}
