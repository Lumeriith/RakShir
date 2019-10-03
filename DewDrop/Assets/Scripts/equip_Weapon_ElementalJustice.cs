using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_ElementalJustice : Equippable
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 20f;
        owner.stat.baseAttacksPerSecond = 1.3f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Elemental - Stand");
            owner.ChangeWalkAnimation("Walk");
        }
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 0f;
        owner.stat.baseAttacksPerSecond = 0f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Stand");
            owner.ChangeWalkAnimation("Walk");
        }
    }
}
