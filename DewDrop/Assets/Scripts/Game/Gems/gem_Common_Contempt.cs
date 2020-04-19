using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Contempt : Gem
{
    public float[] bonusPercentage = { 15, 20, 25, 30, 35 };

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.isMine) owner.OnDealMagicDamage += DealtMagicDamage;
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.isMine) owner.OnDealMagicDamage -= DealtMagicDamage;
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        if (info.source.trigger != trigger) return;
        if (info.to.IsAffectedBy(StatusEffectType.Slow) ||
            info.to.IsAffectedBy(StatusEffectType.Stun) ||
            info.to.IsAffectedBy(StatusEffectType.Root)||
            info.to.IsAffectedBy(StatusEffectType.Silence)||
            info.to.IsAffectedBy(StatusEffectType.Blind))
        {
            info.source.thing.DoMagicDamage(info.finalDamage * bonusPercentage[level] / 100f, info.to, false, source);
        }
    }
}
