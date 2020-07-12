using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_MinorHealingPotion : Consumable
{
    public float healAmount = 100f;
    public float healDuration = 10f;
    public override void OnUse(CastInfo info)
    {
        SFXManager.CreateSFXInstance("si_cons_AnyPotion", info.owner.transform.position);
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_HealOverTime", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new object[] { healAmount, healDuration });
        DestroySelf();
    }

    public override bool CanBeCast()
    {
        return owner.currentHealth < owner.maximumHealth;
    }

}
