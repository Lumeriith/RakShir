using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Huntress_InfinityGlaive : Equippable
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 30f;
        owner.stat.baseAttacksPerSecond = 1.125f;
        owner.stat.baseDodgeChance += 5f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Huntress - Stand");
            owner.ChangeWalkAnimation("Walk");
        }

    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 1f;
        owner.stat.baseAttacksPerSecond = 1f;
        owner.stat.baseDodgeChance -= 5f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Huntress - Stand");
            owner.ChangeWalkAnimation("Walk");
        }
    }
}
