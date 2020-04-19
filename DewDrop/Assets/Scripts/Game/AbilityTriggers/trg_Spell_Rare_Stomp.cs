using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Stomp : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.25f, false, false, false, false, ChannelSuccess, null));
        StartCooldown();
        SpendMana();
    }

    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_Spell_Rare_Stomp", transform.position, Quaternion.identity);
    }
}
