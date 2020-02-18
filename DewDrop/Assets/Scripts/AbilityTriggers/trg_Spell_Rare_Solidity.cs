using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Solidity : AbilityTrigger
{
    public float healMultiplier = 0.02f;

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
        owner.DoHeal(owner.maximumHealth * healMultiplier, owner, true, source);
        //SFXManager.CreateSFXInstance("si_Spell_Rare_Solidity", owner.transform.position);
        StartCooldown();
    }
}
