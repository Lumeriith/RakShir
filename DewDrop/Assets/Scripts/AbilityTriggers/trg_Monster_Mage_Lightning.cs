using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Mage_Lightning : AbilityTrigger
{
    private Vector3 castPos;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 1.05f, false, false, false, false, ChannelSuccess, null));
        castPos = info.target.transform.position;
        StartCooldown();
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_Mage_Lightning", castPos, Quaternion.identity, info, null);
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.75f, false, false, false, false, null, null));
    }
}
