using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_HarvestSoul : AbilityInstance
{
    public float delay = 1.5f;
    public float damageRatio = 0.2f;
    public float healAmount = 100f;
    public float manahealAmount = 200f;
    public float gracePeriod = 0.5f;
    public float cooldownReductionAmount = 80f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.position = info.target.transform.position;
        transform.parent = info.target.transform;
        if (!photonView.IsMine) return;
        StartCoroutine(CoroutineHarvestSoul());
    }

    private IEnumerator CoroutineHarvestSoul()
    {
        yield return new WaitForSeconds(delay);
        if(info.target.type == LivingThingType.Monster && (info.target.tier == LivingThingTier.Boss || info.target.tier == LivingThingTier.Elite))
        {
            info.owner.DoPureDamage(info.target, info.target.maximumHealth * damageRatio * 0.5f, reference);
        }
        else
        {
            info.owner.DoPureDamage(info.target, info.target.maximumHealth * damageRatio, reference);
        }
        info.owner.DoHeal(info.owner, healAmount, false, reference);
        info.owner.DoManaHeal(info.owner, manahealAmount, false, reference);
        SFXManager.CreateSFXInstance("si_Spell_Rare_HarvestSoul Hit", transform.position);
        float start = Time.time;
        while(Time.time - start < gracePeriod)
        {
            yield return new WaitForSeconds(0.05f);
            if (info.target.IsDead())
            {
                if(info.owner.control.skillSet[4] != null && info.owner.control.skillSet[4] as trg_Spell_Rare_HarvestSoul != null)
                {
                    info.owner.control.skillSet[4].ApplyCooldownReduction(cooldownReductionAmount);
                }
                break;
            }
        }
        
        Despawn();
    }
}
