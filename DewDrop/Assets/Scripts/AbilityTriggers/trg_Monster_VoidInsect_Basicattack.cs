using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_VoidInsect_Basicattack : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, ChannelFinished, null), true);
        StartCooldown(true);
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Monster_VoidInsect_Basicattack", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity);
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.3f, false, true, false, false, null, null), true);
    }
}
