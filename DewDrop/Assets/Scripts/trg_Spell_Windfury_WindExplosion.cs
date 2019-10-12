using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Windfury_WindExplosion : AbilityTrigger
{
    private CastInfo info;

    public override void OnCast(CastInfo info)
    {
        this.info = info;
        Channel channel = new Channel(selfValidator, 0.2f, false, false, false, false, Success, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void Success()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Windfury_WindExplosion", info.owner.transform.position, Quaternion.identity, info);
    }
}
