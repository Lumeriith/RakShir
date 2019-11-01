using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_CursedDagger : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Huntress_CursedDagger", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info);
        StartCooldown();
        SpendMana();
    }
}
