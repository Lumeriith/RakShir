using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Reptile_Swipe : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        StartCooldown();
        SpendMana();
        CreateAbilityInstance("ai_Spell_Reptile_Swipe", owner.transform.position, Quaternion.identity, info);
    }
}
