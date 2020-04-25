using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Rare_CitrineRing : Equipment
{

    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
    }
}
