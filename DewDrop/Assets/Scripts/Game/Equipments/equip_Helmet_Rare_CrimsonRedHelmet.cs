using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_CrimsonRedHelmet : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusHealthRegenerationPerSecond += 2.5f;
        owner.stat.bonusAttackSpeedPercentage += 20f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusHealthRegenerationPerSecond -= 2.5f;
        owner.stat.bonusAttackSpeedPercentage -= 20f;
    }
}
