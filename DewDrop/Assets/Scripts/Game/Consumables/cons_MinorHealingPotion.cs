using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_MinorHealingPotion : Consumable
{
    public float healAmount = 100f;
    public float healDuration = 10f;
    public override bool OnUse(CastInfo info)
    {
        SFXManager.CreateSFXInstance("si_cons_AnyPotion", info.owner.transform.position);
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_HealOverTime", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new SourceInfo(), new object[] { healAmount, healDuration });
        DestroySelf();
        return true;
    }

    public override bool IsReady()
    {
        return owner.currentHealth < owner.maximumHealth;
    }

}
