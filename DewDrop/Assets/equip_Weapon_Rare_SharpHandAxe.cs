using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_SharpHandAxe : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 70f;
        owner.stat.baseAttacksPerSecond = 0.9f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - SharpHandAxe Stand");
            //owner.ChangeWalkAnimation("Rare - SharpHandAxe Walk");
            owner.ChangeWalkAnimation("Walk");
        }
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 1f;
        owner.stat.baseAttacksPerSecond = 1f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Stand");
            owner.ChangeWalkAnimation("Walk");
        }
    }
}
