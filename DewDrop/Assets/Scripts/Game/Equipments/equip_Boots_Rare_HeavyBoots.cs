using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Boots_Rare_HeavyBoots : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 150f;
        owner.stat.bonusMovementSpeed += 30f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 150f;
        owner.stat.bonusMovementSpeed -= 30f;
    }
}
