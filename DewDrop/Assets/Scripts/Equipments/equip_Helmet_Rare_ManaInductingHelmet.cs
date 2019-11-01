using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_ManaInductingHelmet : Equipment
{
    public override void OnEquip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusManaRegenerationPerSecond += 2.5f;
        owner.stat.bonusHealthRegenerationPerSecond += 4f;
    }

    public override void OnUnequip(LivingThing owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusManaRegenerationPerSecond -= 2.5f;
        owner.stat.bonusHealthRegenerationPerSecond -= 4f;
    }
}
