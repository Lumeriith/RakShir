using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_LightOfHigherBeing : AbilityInstance
{
    public float invulnerableDuration = 1.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info.owner.ApplyStatusEffect(StatusEffect.Invulnerable(source, invulnerableDuration));
        DestroySelf(5f);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
