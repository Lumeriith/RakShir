using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_CleansingFlame : AbilityInstance
{
    public float delay = 0.75f;

    public float radius = 1.5f;

    public float healAmount = 60f;
    public float damage = 90f;
    public float silenceDuration = 0.75f;

    public float healthThreshold = 0.5f;
    public float bonusMultiplier = 1.5f;

    public TargetValidator healValidator;
    public TargetValidator damageValidator;

    private ParticleSystem pre;
    private ParticleSystem start;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        pre = transform.Find<ParticleSystem>("Pre");
        start = transform.Find<ParticleSystem>("Start");
        pre.Play();
        StartCoroutine("CoroutineExplode");
    }

    IEnumerator CoroutineExplode()
    {
        yield return new WaitForSeconds(delay);
        start.Play();
        if (isMine)
        {
            SFXManager.CreateSFXInstance("si_Spell_Rare_CleansingFlame", transform.position);

            List<LivingThing> healTargets = info.owner.GetAllTargetsInRange(transform.position, radius, healValidator);
            List<LivingThing> damageTargets = info.owner.GetAllTargetsInRange(transform.position, radius, damageValidator);

            for (int i = 0; i < healTargets.Count; i++)
            {
                if (healTargets[i].currentHealth / healTargets[i].maximumHealth < healthThreshold)
                    info.owner.DoHeal(healAmount * bonusMultiplier, healTargets[i]);
                else
                    info.owner.DoHeal(healAmount, healTargets[i]);
            }
            for (int i = 0; i < damageTargets.Count; i++)
            {
                if (damageTargets[i].currentHealth / damageTargets[i].maximumHealth < healthThreshold)
                    info.owner.DoMagicDamage(damage * bonusMultiplier, damageTargets[i]);
                else
                    info.owner.DoMagicDamage(damage, damageTargets[i]);
                damageTargets[i].ApplyStatusEffect(StatusEffect.Silence(info.owner, silenceDuration));
            }

            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
