using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_VoidSlug_Bite : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.56f, false, false, false, false, ChannelFinished, null));
        StartCooldown();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Monster_VoidSlug_Bite", transform.position, Quaternion.identity);
    }
}
