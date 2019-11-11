using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Rare_AmethystRing : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusMaximumMana += 50f;
        owner.stat.bonusSpellPower += 10f;
    }
    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusMaximumMana -= 50f;
        owner.stat.bonusSpellPower -= 10f;
    }
}
