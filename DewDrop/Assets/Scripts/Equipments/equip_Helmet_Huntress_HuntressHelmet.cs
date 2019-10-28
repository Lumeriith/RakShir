using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Huntress_HuntressHelmet : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusDodgeChance += 10f;
        owner.stat.bonusMaximumHealth += 50f;
        
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusDodgeChance -= 10f;
        owner.stat.bonusMaximumHealth -= 50f;
    }





}


