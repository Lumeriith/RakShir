using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Preservation : Gem
{
    public float[] shieldPercentage = { 20, 35, 50, 65, 80 };
    public float shieldDuration = 4f;

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDoHeal += DidHeal;
        }
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDoHeal -= DidHeal;
        }
    }

    private void DidHeal(InfoHeal info)
    {
        if (info.source.trigger != trigger) return;
        info.to.ApplyStatusEffect(StatusEffect.Shield(source, shieldDuration, shieldPercentage[level]));
    }
}
