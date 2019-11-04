using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_AuraOfPain : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.4f, false, false, false, false, null, null));
        StartCooldown();
        SpendMana();
        CreateAbilityInstance("ai_Spell_Rare_AuraOfPain", info.owner.transform.position, Quaternion.identity);
    }
}
