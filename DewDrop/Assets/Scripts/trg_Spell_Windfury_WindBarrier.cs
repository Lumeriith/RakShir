using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Windfury_WindBarrier : AbilityTrigger
{
    private CastInfo info;

    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0f, true, true, true, false, Success, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void Success()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Windfury_WindBarrier", owner.transform.position + owner.GetCenterOffset(), Quaternion.identity, info);
    }
}
