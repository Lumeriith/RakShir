using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_ReptileClaw : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.baseAttackDamage = 55f;
        owner.stat.baseAttacksPerSecond = 0.8f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Reptile - Stand");
            owner.ChangeWalkAnimation("Reptile - Walk");
        }
    }

    public override void OnUnequip(Entity owner)
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
