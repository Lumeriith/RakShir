using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Solidity : AbilityTrigger
{
    public float shieldDuration = 4f;
    public float shieldAmountHealthMultiplier = 0.15f;

    public override void OnCast(CastInfo info)
    {

    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnTakeDamage += TakeDamage;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnTakeDamage -= TakeDamage;
    }



    private void TakeDamage(InfoDamage info)
    {
        if (!isCooledDown) return;
        owner.statusEffect.ApplyStatusEffect(StatusEffect.Shield(owner, shieldDuration, owner.maximumHealth * shieldAmountHealthMultiplier, true));
        StartCooldown();
    }
}
