using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_PursuitingHood : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 50f;
        owner.stat.bonusAttackSpeedPercentage += 10f;
        owner.stat.bonusMovementSpeed += 30f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 50f;
        owner.stat.bonusAttackSpeedPercentage -= 10f;
        owner.stat.bonusMovementSpeed -= 30f;
    }
}
