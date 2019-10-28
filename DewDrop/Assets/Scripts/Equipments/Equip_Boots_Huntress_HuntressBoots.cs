using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip_Boots_Huntress_HuntressBoots : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed += 70f;
        owner.stat.bonusDodgeChance += 5f;
        owner.stat.bonusMaximumHealth += 50f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed -= 70f;
        owner.stat.bonusDodgeChance -= 5f;
        owner.stat.bonusMaximumHealth -= 50f;
    }
}
