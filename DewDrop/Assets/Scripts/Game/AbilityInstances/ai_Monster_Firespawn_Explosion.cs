using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Firespawn_Explosion : AbilityInstance
{
    public float damage = 50f;
    public float stunDuration = 1.5f;
    public TargetValidator targetValidator;
    private ParticleSystem explode;
    private ParticleSystem start;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        explode = transform.Find("Explode").GetComponent<ParticleSystem>();
        StartCoroutine("CoroutineExplode");
        
    }

    IEnumerator CoroutineExplode()
    {
        start.Play();
        yield return new WaitForSeconds(1f);
        explode.Play();
        if (photonView.IsMine)
        {
            List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, 1.25f, targetValidator);
            for(int i = 0; i < targets.Count; i++)
            {
                targets[i].statusEffect.ApplyStatusEffect(StatusEffect.Stun(source, stunDuration));
                info.owner.DoMagicDamage(damage, targets[i], false, source);
            }
        }
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
