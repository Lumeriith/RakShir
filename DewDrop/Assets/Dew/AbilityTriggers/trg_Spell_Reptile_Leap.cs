using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Reptile_Leap : AbilityTrigger
{
    public float channelTime = 0.25f;
    public override void OnCast(CastInfo info)
    {
        owner.control.StartChanneling(new Channel(selfValidator, channelTime, false, false, false, false, ChannelFinished, ChannelCanceled));
        StartCooldown();
        SpendMana();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Reptile_Leap", owner.transform.position, owner.transform.rotation);
    }
    
    private void ChannelCanceled()
    {
        ResetCooldown();
        RefundMana();
    }
}
