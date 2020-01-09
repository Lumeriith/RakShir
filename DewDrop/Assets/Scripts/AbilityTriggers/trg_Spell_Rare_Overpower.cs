using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Overpower : AbilityTrigger
{
    public float stunDuration = 1f;
    public float damage = 50f;

    public override void OnCast(CastInfo info)
    {

    }

    public override void OnEquip()
    {
        if(owner.photonView.IsMine) owner.OnDoBasicAttackHit += BasicAttackHit;
    }


    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit -= BasicAttackHit;
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        if (!isCooledDown) return;
        info.to.statusEffect.ApplyStatusEffect(StatusEffect.Stun(info.from, stunDuration));
        info.from.DoMagicDamage(damage, info.to);
        SFXManager.CreateSFXInstance("si_Spell_Rare_Overpower", transform.position);
        StartCooldown();
    }
}
