using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_AgileMove : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Huntress_AgileMove", transform.position, Quaternion.identity, info);
        StartCooldown();
        SpendMana();
    }
}
