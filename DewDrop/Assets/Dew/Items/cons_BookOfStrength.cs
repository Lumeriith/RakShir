﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cons_BookOfStrength : Consumable
{
    public override void OnUse(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_cons_BookOfStrength", info.owner.transform.position, Quaternion.identity, info);
        SFXManager.CreateSFXInstance("si_cons_BookOfStrength", info.owner.transform.position);
        DestroySelf();
    }

    public override InfoTextIcon infoTextIcon => InfoTextIcon.Book;
}
