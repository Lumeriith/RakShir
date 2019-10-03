using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_ElementalIntegrity : Equippable
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond += 3f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond -= 3f;
    }
}
