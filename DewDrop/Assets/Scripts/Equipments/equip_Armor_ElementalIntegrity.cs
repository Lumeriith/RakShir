using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_ElementalIntegrity : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond += 7.25f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond -= 7.25f;
    }
}
