using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_HealingPotion : Consumable
{
    public override bool OnUse(CastInfo info)
    {
        if (owner.currentHealth >= owner.maximumHealth) return false;


        owner.DoHeal(100, owner, true);
        return true;
    }
}
