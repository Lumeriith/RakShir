using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Breakthrough : AbilityTrigger
{
    public float channelDuration = 0.25f;

    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, ChannelFinished, ChannelCanceled));
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Rare_Breakthrough", info.owner.transform.position, info.owner.transform.rotation, info);
        SpendMana();
        StartCooldown();
    }

    private void ChannelCanceled()
    {
        
    }
}
