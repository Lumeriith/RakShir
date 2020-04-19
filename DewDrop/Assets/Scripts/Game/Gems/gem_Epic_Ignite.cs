using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Epic_Ignite : Gem
{
    public float damageDuration = 3f;
    public float[] damageAmount = { 50f, 80f, 110f };

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnTriggerCast(bool isMine)
    {
        if (!isMine) return;
        if (owner.control.skillSet[0] != null) owner.control.skillSet[0].ResetCooldown();
        if (!IsAnyInstanceActive()) CreateAbilityInstance("ai_Gem_Epic_Ignite", owner.transform.position, Quaternion.identity);
        
    }
}
