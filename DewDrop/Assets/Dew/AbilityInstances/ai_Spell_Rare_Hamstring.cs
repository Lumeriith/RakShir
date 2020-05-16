using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Hamstring : AbilityInstance
{
    public float damage = 30f;
    public float slowDuration = 2.5f;
    public float slowAmount = 30f;
    public float healAmountMultiplier = 0.075f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {
            SFXManager.CreateSFXInstance("si_Spell_Rare_Hamstring", transform.position);
            info.owner.DoMagicDamage(info.target, damage, false, this);
            info.target.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), this);
            info.owner.DoHeal(info.owner, healAmountMultiplier * info.owner.maximumHealth, false, this);
            
            Despawn();
        }
    }
}
