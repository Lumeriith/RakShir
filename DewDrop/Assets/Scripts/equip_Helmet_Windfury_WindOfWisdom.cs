using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Windfury_WindOfWisdom : Equippable
{
    public float bonusMovementSpeed = 50f;
    public float bonusSpellPower = 20f;
    public float bonusCooldownReduction = 5f;

    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed += bonusMovementSpeed;
        owner.stat.bonusSpellPower += bonusSpellPower;
        owner.stat.bonusCooldownReduction += bonusCooldownReduction;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMovementSpeed -= bonusMovementSpeed;
        owner.stat.bonusSpellPower -= bonusSpellPower;
        owner.stat.bonusCooldownReduction -= bonusCooldownReduction;
    }
}
