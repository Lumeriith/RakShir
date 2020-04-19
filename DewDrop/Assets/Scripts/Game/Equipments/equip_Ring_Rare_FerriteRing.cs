using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Rare_FerriteRing : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 150f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 150f;
    }
}
