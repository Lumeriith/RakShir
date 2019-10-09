using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Boots_ElementalDetermination : Equippable
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed += 50f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed -= 50f;
    }
}
