using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_ProtectionOfAgate : AbilityInstance
{
    public float protectedDuration = 2.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info.owner.ApplyStatusEffect(StatusEffect.Protected(source, protectedDuration));
        DestroySelf(5f);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
