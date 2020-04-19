using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ShieldBash : AbilityTrigger
{
    public float channelDuration = 0.1f;
    public override void OnCast(CastInfo info)
    {
        owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, ChannelFinished, null));
    }

    private void ChannelFinished()
    {
        SpendMana();
        StartCooldown();
        CreateAbilityInstance("ai_Spell_Rare_ShieldBash", owner.transform.position, info.directionQuaternion, info);
    }
}
