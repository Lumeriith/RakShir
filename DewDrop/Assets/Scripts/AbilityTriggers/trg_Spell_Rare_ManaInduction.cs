using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ManaInduction : AbilityTrigger
{
    public float shieldDuration = 4f;
    public float shieldAmountMultiplier = 0.35f;

    private StatusEffect shield = null;

    public override void OnCast(CastInfo info)
    {

    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDealMagicDamage += DealMagicDamage;
    }

    private void Update()
    {
        if(shield == null || !shield.isAlive)
        {
            SetSpecialFillAmount(0f);
        }
        else
        {
            SetSpecialFillAmount(shield.duration / shieldDuration);
        }

    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDealMagicDamage -= DealMagicDamage;
            if (shield != null && shield.isAlive) shield.Remove();
            shield = null;
        }
    }

    private void DealMagicDamage(InfoMagicDamage info)
    {
        float shieldAmount = info.finalDamage * shieldAmountMultiplier;
        if(shield == null || !shield.isAlive)
        {
            shield = StatusEffect.Shield(owner, shieldDuration, shieldAmount, true);
            owner.statusEffect.ApplyStatusEffect(shield);
        }
        else
        {
            shield.SetDuration(shieldDuration);
            shield.SetParameter((float)shield.parameter + shieldAmount);
        }
    }
}
