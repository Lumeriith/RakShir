using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Boots_Rare_SwiftLightBoots : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 50f;
        owner.stat.bonusMovementSpeed += 75f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 50f;
        owner.stat.bonusMovementSpeed -= 75f;
    }
}
