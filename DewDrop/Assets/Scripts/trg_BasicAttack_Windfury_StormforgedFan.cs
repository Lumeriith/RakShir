using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Windfury_StormforgedFan : AbilityTrigger
{
    private CastInfo info;
    object[] data;

    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.2f, false, false, true, true, Success, ResetCooldown);
        info.owner.control.StartChanneling(channel);
        StartCooldown(true);
    }

    private void Success()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Windfury_StormforgedFan", info.owner.leftHand.position, Quaternion.identity, info);
    }
}
