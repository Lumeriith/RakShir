using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_LifeLeech : AbilityTrigger
{
    public float healMultiplier = 0.25f;

    public override void OnCast(CastInfo info)
    {
        
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit += BasicAttackHit;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit -= BasicAttackHit;
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        owner.DoHeal(info.damage * healMultiplier, owner, true, source);
    }
}
