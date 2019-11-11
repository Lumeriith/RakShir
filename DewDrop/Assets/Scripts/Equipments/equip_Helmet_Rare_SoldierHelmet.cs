using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_SoldierHelmet : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 150f;
        owner.stat.bonusHealthRegenerationPerSecond += 2f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 150f;
        owner.stat.bonusHealthRegenerationPerSecond -= 2f;
    }
}
