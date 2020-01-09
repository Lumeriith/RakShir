using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ArcanePower : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDealMagicDamage += DealtMagicDamage;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDealMagicDamage -= DealtMagicDamage;
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        if (!isCooledDown) return;
        StartCooldown();
        CreateAbilityInstance("ai_Spell_Rare_ArcanePower", info.to.transform.position, Quaternion.identity);
    }

}
