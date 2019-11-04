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
        info.owner.DoPureDamage(info.target.maximumHealth * damageRatio, info.target);
        float start = Time.time;
        while(Time.time - start < gracePeriod)
        {
            yield return new WaitForSeconds(0.05f);
            if (info.target.IsDead())
            {
                info.owner.DoHeal(healAmount, info.owner);
                info.owner.DoManaHeal(manahealAmount, info.owner);
                if(info.owner.control.skillSet[4] != null && info.owner.control.skillSet[4] as trg_Spell_Rare_HarvestSoul != null)
                {
                    info.owner.control.skillSet[4].ApplyCooldownReduction(cooldownReductionAmount);
                }
                break;
            }
        }
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
