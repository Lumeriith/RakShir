using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Huntress_HuntressHelmet : Equipment
{
    public float cooldownReductionAmount = 1.25f;

    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusDodgeChance += 10f;
        owner.stat.bonusMaximumHealth += 50f;
        if (owner.photonView.IsMine) owner.OnDodge += CooldownReduction;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusDodgeChance -= 10f;
        owner.stat.bonusMaximumHealth -= 50f;
        if (owner.photonView.IsMine) owner.OnDodge -= CooldownReduction;
    }


    private void CooldownReduction(InfoMiss info)
    {
        for(int i= 0; i < owner.control.skillSet.Length; i++)
        {
            if (owner.control.skillSet[i] != null) owner.control.skillSet[i].ApplyCooldownReduction(cooldownReductionAmount);
        }
        
    }


}


