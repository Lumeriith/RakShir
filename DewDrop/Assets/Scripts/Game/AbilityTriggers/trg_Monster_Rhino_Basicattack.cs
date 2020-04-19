using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Rhino_Basicattack : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, ChannelSuccess, null));
        StartCooldown(true);
    }

    void ChannelSuccess()
    {
        CreateAbilityInstance("ai_Monster_Rhino_Basicattack", info.owner.transform.position, Quaternion.identity, info);
    }
}
