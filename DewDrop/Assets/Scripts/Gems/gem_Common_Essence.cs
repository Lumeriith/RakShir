using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Essence : Gem
{
    public float manaPerCast = 5f;
    public float[] manaBonusLimit = { 150f, 250f, 350f, 450f, 550f };
    public float increasedMana = 0f;
    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnTriggerCast(bool isMine)
    {
        if(increasedMana + manaPerCast > manaBonusLimit[level])
        {
            owner.stat.baseMaximumMana += manaBonusLimit[level] - increasedMana;
            increasedMana = manaBonusLimit[level];
        }
        else
        {
            owner.stat.baseMaximumMana += manaPerCast;
            increasedMana += manaPerCast;
        }
    }
}
