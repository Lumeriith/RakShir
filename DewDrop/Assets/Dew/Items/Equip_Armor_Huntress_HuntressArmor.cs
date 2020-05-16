using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip_Armor_Huntress_HuntressArmor : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 300f;
        owner.stat.bonusDodgeChance += 5f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 300f;
        owner.stat.bonusDodgeChance -= 5f;
    }


}
