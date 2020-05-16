using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_BurningGrounds : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.3f, false, false, false, false, ChannelFinished, null));
        StartCooldown();
        SpendMana();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Rare_BurningGrounds", info.point, Quaternion.identity);
    }
}
