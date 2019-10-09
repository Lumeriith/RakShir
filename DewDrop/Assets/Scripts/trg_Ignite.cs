using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Ignite : AbilityTrigger
{

    public override void OnCast(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Ignite", owner.rightHand.position, Quaternion.identity, info);
        StartCooldown();
    }
}
