using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_SwordAndShield : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.baseAttackDamage = 70f;
        owner.stat.baseAttacksPerSecond = 1.2f;
        owner.stat.bonusMaximumHealth += 150f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - SwordAndShield Stand");
            owner.ChangeWalkAnimation("Rare - SwordAndShield Walk");
        }
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.baseAttackDamage = 1f;
        owner.stat.baseAttacksPerSecond = 1f;
        owner.stat.bonusMaximumHealth -= 150f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation();
            owner.ChangeWalkAnimation();
        }
    }
}
