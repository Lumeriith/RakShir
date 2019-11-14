using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_JoyOfHunting : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_JoyOfHunting", transform.position, Quaternion.identity);
        StartCooldown();
        SpendMana();
    }

    public override bool IsReady()
    {
        return !IsAnyInstanceActive();
    }
}
