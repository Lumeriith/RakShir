using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_MinorHealingPotion : Consumable
{
    public float healMultiplier = 0.20f;
    public float healDuration = 10f;
    public override bool OnUse(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_HealOverTime", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new object[] { info.owner.maximumHealth * healMultiplier, healDuration });
        DestroySelf();
        return true;
    }
}
