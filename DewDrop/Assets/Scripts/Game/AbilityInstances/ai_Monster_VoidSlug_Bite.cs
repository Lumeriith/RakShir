using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_VoidSlug_Bite : AbilityInstance
{
    public float poisonDuration = 8f;
    public float poisonDamage = 70f;
    public float slowDuration1 = 1f;
    public float slowDuration2 = 0.5f;
    public float slowDuration3 = 1.5f;
    public float slowAmount = 20f;
    private ParticleSystem spray;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.position = info.owner.head.position;
        transform.rotation = info.owner.head.rotation;
        spray = transform.Find("Spray").GetComponent<ParticleSystem>();
        spray.Play();
        if (photonView.IsMine)
        {
            info.target.ApplyStatusEffect(StatusEffect.DamageOverTime(source, poisonDuration, poisonDamage));
            info.target.ApplyStatusEffect(StatusEffect.Slow(source, slowDuration1, slowAmount));
            info.target.ApplyStatusEffect(StatusEffect.Slow(source, slowDuration2, slowAmount));
            info.target.ApplyStatusEffect(StatusEffect.Slow(source, slowDuration3, slowAmount));
        }
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
