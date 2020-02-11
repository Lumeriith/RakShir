using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_WaveOfPain : AbilityTrigger
{
    public float channelDuration = 0.25f;

    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, ChannelFinished));
    }

    private void ChannelFinished()
    {
        SpendMana();
        StartCooldown();
        CreateAbilityInstance("ai_Spell_Rare_WaveOfPain", transform.position, info.directionQuaternion, info);
    }
}
