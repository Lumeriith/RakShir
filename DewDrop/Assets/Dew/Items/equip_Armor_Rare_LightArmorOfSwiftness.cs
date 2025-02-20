﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Armor_Rare_LightArmorOfSwiftness : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 250f;
        owner.stat.bonusMovementSpeed += 20f;
        owner.stat.bonusHealthRegenerationPerSecond += 1.5f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 250f;
        owner.stat.bonusMovementSpeed -= 20f;
        owner.stat.bonusHealthRegenerationPerSecond -= 1.5f;
    }
}
