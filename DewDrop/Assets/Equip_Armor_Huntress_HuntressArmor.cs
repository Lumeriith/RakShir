using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip_Armor_Huntress_HuntressArmor : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 300f;
        owner.stat.bonusDodgeChance += 5f;
        if (owner.photonView.IsMine)
        {
            owner.OnDodge += Dodged;
        }
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 300f;
        owner.stat.bonusDodgeChance -= 5f;
        if (owner.photonView.IsMine)
        {
            owner.OnDodge -= Dodged;
        }
    }

    private void Dodged(InfoMiss info)
    {
        if (owner.control.skillSet[1] != null) owner.control.skillSet[1].ApplyCooldownReduction(1.5f);
    }
}
