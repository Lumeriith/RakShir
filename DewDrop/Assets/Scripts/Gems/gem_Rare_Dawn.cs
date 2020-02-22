using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Dawn : Gem
{
    public TargetValidator affectedTargets;
    public float explosionDelay = 1.5f;
    public float explosionRadius = 2.5f;
    public float[] damageAmount = { 35, 45, 55, 65 };

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {

    }
    public override void OnTriggerCast(bool isMine)
    {
        if (isMine) CreateAbilityInstance("ai_Gem_Rare_Dawn", owner.transform.position, Quaternion.identity);
    }
}
