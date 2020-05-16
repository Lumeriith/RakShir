using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_ElementalIntegrity : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond += 3.25f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusHealthRegenerationPerSecond -= 3.25f;
    }
}
