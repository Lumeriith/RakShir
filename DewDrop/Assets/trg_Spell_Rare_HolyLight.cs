using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_HolyLight : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_HolyLight", transform.position, Quaternion.identity);
        StartCooldown();
        SpendMana();
    }
}
