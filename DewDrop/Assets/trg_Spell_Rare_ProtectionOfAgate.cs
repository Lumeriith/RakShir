using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ProtectionOfAgate : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_ProtectionOfAgate", transform.position, Quaternion.identity);
        StartCooldown();
    }
}
