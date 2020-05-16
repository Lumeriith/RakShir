using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_FreezingCloak : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 150f;
        owner.stat.bonusMaximumMana += 75f;
        owner.stat.bonusManaRegenerationPerSecond += 3.5f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 150f;
        owner.stat.bonusMaximumMana -= 75f;
        owner.stat.bonusManaRegenerationPerSecond -= 3.5f;
    }
}
