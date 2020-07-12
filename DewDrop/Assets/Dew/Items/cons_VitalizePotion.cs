using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_VitalizePotion : Consumable
{

    public float missingHealthHealMultiplier = 0.40f;
    public float healDuration = 4f;
    public override void OnUse(CastInfo info)
    {
        SFXManager.CreateSFXInstance("si_cons_AnyPotion", info.owner.transform.position);
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_HealOverTime", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new object[] { (info.owner.maximumHealth - info.owner.currentHealth) * missingHealthHealMultiplier, healDuration });
        DestroySelf();
    }

    public override bool CanBeCast()
    {
        return owner.currentHealth < owner.maximumHealth;
    }
}
