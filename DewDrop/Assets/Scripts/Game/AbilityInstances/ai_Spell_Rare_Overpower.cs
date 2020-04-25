using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Overpower : AbilityInstance
{
    public float stunDuration = 1f;
    public float damage = 50f;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        if (!photonView.IsMine) return;
        SFXManager.CreateSFXInstance("si_Spell_Rare_Overpower", transform.position);
        info.owner.DoMagicDamage(info.target, damage, false, reference);
        info.target.ApplyStatusEffect(StatusEffect.Stun(stunDuration), reference);
        
        Despawn();
    }
}
