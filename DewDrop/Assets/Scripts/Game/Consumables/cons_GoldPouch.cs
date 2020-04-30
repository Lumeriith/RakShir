using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_GoldPouch : Consumable
{
    public float goldAmount;
    public override bool OnUse(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_GoldPouch", info.owner.transform.position, Quaternion.identity, info, new object[] { goldAmount });
        SFXManager.CreateSFXInstance("si_cons_GoldPouch", info.owner.transform.position);
        DestroySelf();
        return true;
    }

    public override InfoTextIcon infoTextIcon => InfoTextIcon.Money;
}
