using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_ManaPotion : Consumable
{
    public float manahealAmount = 100f;
    public override void OnUse(CastInfo info)
    {
        SFXManager.CreateSFXInstance("si_cons_AnyPotion", info.owner.transform.position);
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_ManaHeal", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info, new object[] { manahealAmount });
    }

    public override bool CanBeCast()
    {
        return owner.stat.currentMana < owner.stat.finalMaximumMana;
    }
}
