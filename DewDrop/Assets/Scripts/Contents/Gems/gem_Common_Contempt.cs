using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Contempt : Gem
{
    public float[] bonusPercentage = { 15, 20, 25, 30, 35 };

    public override void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstance instance)
    {
        if (isMine) instance.OnDealMagicDamage += DealtMagicDamage;
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        if (info.to.IsAffectedBy(StatusEffectType.Slow) ||
            info.to.IsAffectedBy(StatusEffectType.Stun) ||
            info.to.IsAffectedBy(StatusEffectType.Root)||
            info.to.IsAffectedBy(StatusEffectType.Silence)||
            info.to.IsAffectedBy(StatusEffectType.Blind))
        {
            owner.DoMagicDamage(info.to, info.finalDamage * bonusPercentage[level] / 100f, false, this);
        }
    }
}
