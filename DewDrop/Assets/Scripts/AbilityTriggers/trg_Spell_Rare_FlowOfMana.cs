using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_FlowOfMana : AbilityTrigger
{
    public float manaHealAmount = 5f;

    public override void OnCast(CastInfo info)
    {
        
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDealMagicDamage += DealMagicDamage;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDealMagicDamage -= DealMagicDamage;
    }

    private void DealMagicDamage(InfoMagicDamage info)
    {
        owner.DoManaHeal(manaHealAmount, owner);
    }
}
