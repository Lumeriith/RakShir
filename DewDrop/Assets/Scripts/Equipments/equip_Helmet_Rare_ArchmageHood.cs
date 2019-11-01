using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_ArchmageHood : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumMana += 300f;
        owner.stat.bonusSpellPower += 10f;
        owner.stat.bonusCooldownReduction += 10f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumMana -= 300f;
        owner.stat.bonusSpellPower -= 10f;
        owner.stat.bonusCooldownReduction -= 10f;
    }
}
