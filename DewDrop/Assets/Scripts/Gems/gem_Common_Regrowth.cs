using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Regrowth : Gem
{
    public float[] healAmount = { 20, 30, 40, 50, 60 };

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger) { }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger) { }

    public override void OnTriggerCast(bool isMine)
    {
        if (isMine) CreateAbilityInstance("ai_Gem_Common_Regrowth", owner.transform.position, Quaternion.identity);
    }
}
