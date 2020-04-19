using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_OverpoweringHelmet : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusHealthRegenerationPerSecond += 1.5f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusHealthRegenerationPerSecond -= 1.5f;
    }
}
