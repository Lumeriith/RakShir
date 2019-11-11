using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Firespawn_Basicattack : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, true, false, true, ChannelFinished, null, true));
        StartCooldown(true);
    }

    private void ChannelFinished()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_Firespawn_Basicattack", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info);
    }
}
