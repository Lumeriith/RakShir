using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Aftertaste : Gem
{
    public float[] bonusMagicDamagePercentage = { 10, 15, 20, 25, 30 };
    public float[] bonusHealPercentage = { 20, 25, 30, 35, 40 };

    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDealMagicDamage += DealtMagicDamage;
            owner.OnDoHeal += DidHeal;
        }
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDealMagicDamage -= DealtMagicDamage;
            owner.OnDoHeal -= DidHeal;
        }
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        if (info.source.trigger != trigger) return;
        CastInfo castInfo = new CastInfo { owner = owner, target = info.to };
        CreateAbilityInstance("ai_Gem_Common_Aftertaste", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, castInfo, new object[] { true, info.finalDamage });
    }

    private void DidHeal(InfoHeal info)
    {
        if (info.source.trigger != trigger) return;
        CastInfo castInfo = new CastInfo { owner = owner, target = info.to };
        CreateAbilityInstance("ai_Gem_Common_Aftertaste", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, castInfo, new object[] { false, info.finalHeal });
    }


}
