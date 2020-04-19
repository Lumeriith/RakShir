using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Huntress_HuntressRing : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusAgility += 3f;
    
}

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusAgility -= 3f;
    }
}
