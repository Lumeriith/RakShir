using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Overdrive : Gem
{
    public float increasedManaCost = 5f;
    public float[] cooldownReduction = { 1.0f, 1.5f, 2.0f, 2.5f };

    private float reducedCooldown;

    public override void OnEquip(Entity owner, AbilityTrigger trigger)
    {
        if(trigger.cooldownTime <= 1)
        {
            reducedCooldown = 0;
        }
        else if (trigger.cooldownTime - cooldownReduction[level] < 1)
        {
            reducedCooldown = trigger.cooldownTime - 1;
            trigger.cooldownTime -= reducedCooldown;
        }
        else
        {
            reducedCooldown = cooldownReduction[level];
            trigger.cooldownTime -= reducedCooldown;
        }

        trigger.manaCost += 5f;
    }

    public override void OnUnequip(Entity owner, AbilityTrigger trigger)
    {
        trigger.cooldownTime += reducedCooldown;
        trigger.manaCost -= 5f;
        reducedCooldown = 0f;
    }
}
