using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_CloakOfHealer : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusHealthRegenerationPerSecond += 2.5f;
        owner.stat.bonusManaRegenerationPerSecond += 4f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusHealthRegenerationPerSecond -= 2.5f;
        owner.stat.bonusManaRegenerationPerSecond -= 4f;
    }
}
