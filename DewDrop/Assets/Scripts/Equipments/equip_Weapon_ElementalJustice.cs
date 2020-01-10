using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_ElementalJustice : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 25f;
        owner.stat.baseAttacksPerSecond = 1.6f;
        if (photonView.IsMine)
        {
            if (name.Contains("Reptile"))
            {
                owner.ChangeStandAnimation("Reptile - Stand");
                owner.ChangeWalkAnimation("Reptile - Walk");
            }
            else
            {
                owner.ChangeStandAnimation("Elemental - Stand");
                owner.ChangeWalkAnimation("Walk");
            }

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
