using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_CrimsonRedHelmet : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 75f;
        owner.stat.bonusHealthRegenerationPerSecond += 5f;
        owner.stat.bonusAttackSpeedPercentage += 20f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 75f;
        owner.stat.bonusHealthRegenerationPerSecond -= 5f;
        owner.stat.bonusAttackSpeedPercentage -= 20f;
    }
}
