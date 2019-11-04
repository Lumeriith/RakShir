using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_FreezingCloak : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusMaximumMana += 75f;
        owner.stat.bonusManaRegenerationPerSecond += 3.5f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusMaximumMana -= 75f;
        owner.stat.bonusManaRegenerationPerSecond -= 3.5f;
    }
}
