using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Overpower : AbilityTrigger
{


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
        CastInfo castInfo = new CastInfo { owner = info.from, target = info.to };
        CreateAbilityInstance("ai_Spell_Rare_Overpower", info.to.transform.position, Quaternion.identity, castInfo);
        StartCooldown();
    }
}
