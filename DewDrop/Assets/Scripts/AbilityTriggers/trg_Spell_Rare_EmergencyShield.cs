using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_EmergencyShield : AbilityTrigger
{
    public float healthThreshold = 0.4f;
    public float shieldDuration = 6f;
    public float shieldAmountManaMultiplier = 1f;
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
        if(owner.currentHealth <= owner.maximumHealth * healthThreshold)
        {
            owner.statusEffect.ApplyStatusEffect(StatusEffect.Shield(source, shieldDuration, owner.stat.finalMaximumMana * shieldAmountManaMultiplier, true));
            SFXManager.CreateSFXInstance("si_Spell_Rare_EmergencyShield", owner.transform.position);
            StartCooldown();
        }
            
    }
}
