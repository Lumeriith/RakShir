using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_Evade : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Huntress_Evade", info.owner.transform.position, Quaternion.identity, info);
        StartCooldown();
        SpendMana();
    }
}
