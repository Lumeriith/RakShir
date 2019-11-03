using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_VoidSlug_Shout : AbilityTrigger
{
    public float channelDuration;

    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, null, null));
        StartCooldown();
    }
}
