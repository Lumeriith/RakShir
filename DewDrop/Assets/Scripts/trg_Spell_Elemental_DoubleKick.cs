using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Elemental_DoubleKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        StartCooldown();
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_DoubleKick", owner.transform.position, Quaternion.identity, info);
    }


}
