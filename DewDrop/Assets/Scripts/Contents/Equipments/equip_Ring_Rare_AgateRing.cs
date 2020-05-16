using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Rare_AgateRing : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 150f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 150f;
    }
}
