using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Frost : Gem
{
    public float[] slowDuration = { 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };
    public float[] slowAmount = { 20, 25, 30, 35, 40 };

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
        CreateAbilityInstance("ai_Gem_Common_Frost", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, CastInfo.OwnerAndTarget(owner, info.to));
    }
}
