using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Hamstring : AbilityInstance
{
    public float damage = 30f;
    public float slowDuration = 2.5f;
    public float slowAmount = 30f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {
            SFXManager.CreateSFXInstance("si_Spell_Rare_Hamstring", transform.position);
            info.owner.DoMagicDamage(damage, info.target);
            info.target.ApplyStatusEffect(StatusEffect.Slow(info.owner, slowDuration, slowAmount));
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
