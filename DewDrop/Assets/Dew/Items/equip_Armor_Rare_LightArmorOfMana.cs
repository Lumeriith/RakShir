using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_LightArmorOfMana : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 250f;
        owner.stat.bonusMaximumMana += 50f;
        owner.stat.bonusHealthRegenerationPerSecond += 2.5f;
    }


    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 250f;
        owner.stat.bonusMaximumMana -= 50f;
        owner.stat.bonusHealthRegenerationPerSecond -= 2.5f;
    }
}
