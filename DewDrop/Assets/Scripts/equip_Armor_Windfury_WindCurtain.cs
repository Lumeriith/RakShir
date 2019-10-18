using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Windfury_WindCurtain : Equippable
{
    public float bonusCooldownReduction;
    public float bonusAgility;

    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusCooldownReduction += this.bonusCooldownReduction;
        owner.stat.bonusAgility += this.bonusAgility;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusCooldownReduction -= this.bonusCooldownReduction;
        owner.stat.bonusAgility -= this.bonusAgility;
    }
}
