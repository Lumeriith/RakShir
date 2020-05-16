using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_HarvestSoul : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.2f, false, false, false, false, null, null));
        StartCooldown();
        SpendMana();
        CreateAbilityInstance("ai_Spell_Rare_HarvestSoul", info.target.transform.position, Quaternion.identity);
    }
}
