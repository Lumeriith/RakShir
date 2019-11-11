using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_FlamingCloak : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 200f;
        owner.stat.bonusMaximumMana += 50f;
        owner.stat.bonusManaRegenerationPerSecond += 3.5f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 200f;
        owner.stat.bonusMaximumMana -= 50f;
        owner.stat.bonusManaRegenerationPerSecond -= 3.5f;
    }


}
