using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_FlowOfMana : AbilityTrigger
{
    public float manaHealAmount = 5f;
    public float cooldownReduction = 1f;
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
        if (!isCooledDown) return;

        owner.DoManaHeal(manaHealAmount, owner);
        for(int i = 1; i < owner.control.cooldownTime.Length; i++)
        {
            owner.control.cooldownTime[i] = Mathf.MoveTowards(owner.control.cooldownTime[i], 0, cooldownReduction);
        }
        StartCooldown();
    }
}
