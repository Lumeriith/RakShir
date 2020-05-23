using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_SharpHandAxe : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.baseAttackDamage = 95f;
        owner.stat.baseAttacksPerSecond = 1.15f;
        owner.stat.bonusMaximumHealth += 100f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - SharpHandAxe Stand");
            owner.ChangeWalkAnimation("Walk");
        }
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.baseAttackDamage = 1f;
        owner.stat.baseAttacksPerSecond = 1f;
        owner.stat.bonusMaximumHealth -= 100f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation();
            owner.ChangeWalkAnimation();
        }
    }
}
