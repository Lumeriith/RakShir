using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Windfury_StormforgedFan : Equippable
{
    public float bonusAttackDamage;
    public float bonusBaseAttackPerSecond;

    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusAttackDamage += bonusAttackDamage;
        owner.stat.bonusAttackSpeedPercentage += bonusBaseAttackPerSecond;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusAttackDamage -= bonusAttackDamage;
        owner.stat.bonusAttackSpeedPercentage -= bonusBaseAttackPerSecond;
    }
}
