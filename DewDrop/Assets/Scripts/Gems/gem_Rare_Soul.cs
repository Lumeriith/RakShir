using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Soul : Gem
{
    public float orbSpeed = 8f;

    public float healthPerKill = 5f;
    public float scarDuration = 3f;

    public float[] healthBonusLimit = { 200f, 300f, 400f, 500f };
    public float increasedHealth = 0f;

    public List<LivingThing> damagedTargets;
    public List<float> damagedTimes;

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
        int index = damagedTargets.IndexOf(info.to);
        if (index == -1)
        {
            damagedTargets.Add(info.to);
            damagedTimes.Add(Time.time);
            CreateAbilityInstance("ai_Gem_Rare_Soul", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, CastInfo.OwnerAndTarget(owner, info.to));
        }
        else
        {
            damagedTimes[index] = Time.time;
        }
        
    }
}
