using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_SwiftLikeWind : AbilityTrigger
{
    public float cooldownReductionAmount;
    public float speedDuration = 1.5f;
    public float speedAmount = 25f;

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDodge += Swift;
    }

    public override void OnCast(CastInfo info)
    {

    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDodge -= Swift;
    }


    private void Swift(InfoMiss info)
    {
        if (!isCooledDown) return;
        
        for (int i = 1; i < owner.control.skillSet.Length; i++)
        {
            if (owner.control.skillSet[i] != null) owner.control.skillSet[i].ApplyCooldownReduction(cooldownReductionAmount);
        }
        StartCooldown();
        owner.statusEffect.ApplyStatusEffect(StatusEffect.Speed(speedDuration, speedAmount), null);
        // TODO better
    }
}
