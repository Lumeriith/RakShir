using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_PoisonDagger : Equipment
{
    public float poisonAmount = 20f;
    public float poisonDuration = 5f;
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 10f;
        owner.stat.baseAttacksPerSecond = 1.2f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - PoisonDagger Stand");
            owner.ChangeWalkAnimation("Walk");
            owner.OnDoBasicAttackHit += BasicAttackHit;
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
            owner.OnDoBasicAttackHit -= BasicAttackHit;
        }
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        info.to.ApplyStatusEffect(StatusEffect.DamageOverTime(info.from, poisonDuration, poisonAmount));
    }
}
