using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Windfury_HurricaneDash : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem fly;
    private ParticleSystem hit;

    public float duration = 0.8f;
    public float dashDistance = 5f;
    public float bonusDashDistancePerMovementSpeed = 0.1f;
    public float damagePerMovementSpeed;
    public float airborneDuration = 0.1f;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        fly.Play();
    }

    protected override void AliveUpdate()
    {
        duration -= Time.deltaTime;

        if (duration >= 0)
        {
            dashDistance = info.owner.stat.finalMovementSpeed * bonusDashDistancePerMovementSpeed;
            info.owner.DashThroughForDuration(info.directionVector * dashDistance, duration);
        }
        else
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null || target == info.owner) { return; }
        info.owner.CancelDash();
        info.owner.DoMagicDamage(info.owner.stat.finalMovementSpeed * (damagePerMovementSpeed / 100), target);
        target.AirborneForDuration(target.transform.position, airborneDuration);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
