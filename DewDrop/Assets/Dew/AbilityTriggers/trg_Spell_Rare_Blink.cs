using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Blink : AbilityTrigger
{
    public float channelTime = 0.1f;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelTime, false, false, false, false, ChannelFinished, null));
        
        StartCooldown();
        SpendMana();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Rare_Blink", transform.position, Quaternion.identity);
    }
}
