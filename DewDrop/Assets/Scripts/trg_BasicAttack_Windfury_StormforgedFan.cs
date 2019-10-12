using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Windfury_StormforgedFan : AbilityTrigger
{
    private CastInfo info;

    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.1f, false, false, false, true, Success, ResetCooldown);
        info.owner.control.StartChanneling(channel);
        StartCooldown(true);
    }

    private void Success()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_BasicAttack_Windfury_StormforgedFan", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info);
    }
}
