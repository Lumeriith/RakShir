using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_FrostArmor : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 250f;
        owner.stat.bonusHealthRegenerationPerSecond += 2f;
        owner.stat.bonusManaRegenerationPerSecond += 2f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 250f;
        owner.stat.bonusHealthRegenerationPerSecond -= 2f;
        owner.stat.bonusManaRegenerationPerSecond -= 2f;
    }
}
