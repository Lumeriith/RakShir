using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_CleansingFlame : AbilityTrigger
{
    public float channelDuration = 0.15f;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, ChannelFinished));
    }

    private void ChannelFinished()
    {
        SpendMana();
        StartCooldown();
        CreateAbilityInstance("ai_Spell_Rare_CleansingFlame", info.point, Quaternion.identity, info);
    }
}
