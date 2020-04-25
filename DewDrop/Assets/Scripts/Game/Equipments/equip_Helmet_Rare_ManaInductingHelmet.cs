using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equip_Helmet_Rare_ManaInductingHelmet : Equipment
{
    public override void OnEquip(Entity owner)
    {
        owner.stat.bonusMaximumHealth += 100f;
        owner.stat.bonusManaRegenerationPerSecond += 3f;
        owner.stat.bonusHealthRegenerationPerSecond += 1.5f;
    }

    public override void OnUnequip(Entity owner)
    {
        owner.stat.bonusMaximumHealth -= 100f;
        owner.stat.bonusManaRegenerationPerSecond -= 3f;
        owner.stat.bonusHealthRegenerationPerSecond -= 1.5f;
    }
}
