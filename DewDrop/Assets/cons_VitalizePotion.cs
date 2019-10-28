using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_VitalizePotion : Consumable
{

    public float missingHealthHealMultiplier = 0.40f;
    public float healDuration = 4f;
    public override bool OnUse(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_HealOverTime", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new object[] { (info.owner.maximumHealth - info.owner.currentHealth) * missingHealthHealMultiplier, healDuration });
        DestroySelf();
        return true;
    }
}
