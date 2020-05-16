using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_VenomKnives : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, 0.3f, false, false, false, false, ChannelSuccess, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
        SpendMana();
    }


    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_Spell_Huntress_VenomKnives", info.owner.transform.position, Quaternion.identity, info);
        Channel channel = new Channel(selfValidator, 0.2f, false, false, false, false, null, null);
        info.owner.control.StartChanneling(channel);
    }
}
