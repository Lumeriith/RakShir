using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_HoodOfLucidity : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 50f;
        owner.stat.bonusMaximumMana += 100f;
        owner.stat.bonusSpellPower += 15f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 50f;
        owner.stat.bonusMaximumMana -= 100f;
        owner.stat.bonusSpellPower -= 15f;
    }
}
