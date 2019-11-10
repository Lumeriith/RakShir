using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Weapon_Rare_StaffOfPain : Equipment
{
    public float painDuration = 4f;
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.baseAttackDamage = 20f;
        owner.stat.baseAttacksPerSecond = 1.4f;
        if (photonView.IsMine)
        {
            owner.ChangeStandAnimation("Rare - MagicBow Stand");
            owner.ChangeWalkAnimation("Rare - MagicBow Walk");
            owner.OnDealMagicDamage += DealMagicDamage;
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
            owner.OnDealMagicDamage -= DealMagicDamage;
        }
    }

    private void DealMagicDamage(InfoMagicDamage info)
    {
        List<StatusEffect> pains = info.to.statusEffect.GetCustomStatusEffectsByName("고통");
        if (pains.Count == 0)
        {
            info.to.ApplyStatusEffect(StatusEffect.Custom(info.from, "고통", painDuration));
        }
        else
        {
            pains[0].SetDuration(painDuration);
        }
    }
}
