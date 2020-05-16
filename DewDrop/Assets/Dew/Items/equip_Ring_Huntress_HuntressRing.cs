using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Ring_Huntress_HuntressRing : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusAgility += 3f;
    
}

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusAgility -= 3f;
    }
}
