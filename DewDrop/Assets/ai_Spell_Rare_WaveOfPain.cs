using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_WaveOfPain : AbilityInstance
{
    public AnimationCurve travelCurve;
    public float travelTime = 0.75f;
    public float travelDistance = 6f;

    public float width = 2.5f;

    public float tickTime = 0.5f;
    public float ticksCount = 5f;

    public float initialDamage = 40f;
    public float tickDamage = 5f;

    public float slowAmount = 25f;
    public float slowDuration = 4f;

    public float slowedEnemiesDamageMultiplier = 2f;

    public TargetValidator targetValidator;

    private ParticleSystem projectile;

    private Vector3 startPosition;
    private Vector3 endPosition;



    private float elapsedTime = 0f;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        projectile = transform.Find<ParticleSystem>("Projectile");
        projectile.Play();
        startPosition = transform.position;
        endPosition = transform.position + info.directionVector * travelDistance;
        StartCoroutine("CoroutineWaveOfPain");
    }

    protected override void AliveUpdate()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, travelCurve.Evaluate(elapsedTime / travelTime));
        }
    }

    IEnumerator CoroutineWaveOfPain()
    {
        yield return new WaitForSeconds(travelTime);
        projectile.Stop();
        GetComponent<SphereCollider>().enabled = false;
        List<LivingThing> targets;
        for(int i = 0; i < ticksCount; i++)
        {
            yield return new WaitForSeconds(tickTime);
            targets = info.owner.GetAllTargetsInLine(startPosition, info.directionVector, width, travelDistance, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                SFXManager.CreateSFXInstance("si_Spell_Rare_WaveOfPain TickHit", targets[j].transform.position);
                if (targets[j].IsAffectedBy(StatusEffectType.Slow))
                {
                    info.owner.DoMagicDamage(tickDamage * slowedEnemiesDamageMultiplier, targets[j]);
                }
                else
                {
                    info.owner.DoMagicDamage(tickDamage, targets[j]);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        if (thing.IsAffectedBy(StatusEffectType.Slow))
        {
            info.owner.DoMagicDamage(initialDamage * slowedEnemiesDamageMultiplier, thing);
        }
        else
        {
            info.owner.DoMagicDamage(initialDamage, thing);
        }
        SFXManager.CreateSFXInstance("si_Spell_Rare_WaveOfPain InitialHit", thing.transform.position);

        thing.ApplyStatusEffect(StatusEffect.Slow(info.owner, slowDuration, slowAmount));
        
    }
}
