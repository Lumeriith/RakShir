using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_MagicBow : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.baseAttackDamage = 50f;
        owner.stat.baseAttacksPerSecond = 1.3f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - MagicBow Stand");
            owner.ChangeWalkAnimation("Rare - MagicBow Walk");
        }
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.baseAttackDamage = 1f;
        owner.stat.baseAttacksPerSecond = 1f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation();
            owner.ChangeWalkAnimation();
        }
    }
}
