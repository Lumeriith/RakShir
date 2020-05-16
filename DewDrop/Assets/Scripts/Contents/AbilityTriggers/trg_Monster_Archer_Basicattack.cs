using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Archer_Basicattack : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, true, ChannelFinished, null, true));
        StartCooldown(true);
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Monster_Archer_Basicattack", info.owner.leftHand.position, Quaternion.identity, info);
    }
}
