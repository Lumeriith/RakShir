using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Windfury_HurricaneDash : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem fly;
    private ParticleSystem hit;
    private Vector3 targetPosition = Vector3.zero;
    private float dashDistance;
    private float airborneDuration;

    public float duration = 0.5f;
    public float damagePerMovementSpeed = 0.3f;
    public float airborneDurationPerMovementSpeed = 0.002f;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        dashDistance = (float)data[0];
        targetPosition = transform.position + info.directionVector * dashDistance;
        print(info.directionVector);
        print(targetPosition);

        airborneDuration = airborneDurationPerMovementSpeed * info.owner.stat.finalMovementSpeed;
        info.owner.DashThroughForDuration(targetPosition, duration);
        fly.Play();
    }

    protected override void AliveUpdate()
    {
        duration -= Time.deltaTime;
        if (duration > 0)
        {
            transform.position = info.owner.GetCenterOffset() + info.owner.transform.position;
        }
        else
        {
            info.owner.CancelDash();
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null || target == info.owner) { return; }
        info.owner.DoMagicDamage(info.owner.stat.finalMovementSpeed * (damagePerMovementSpeed / 100), target);
        target.AirborneForDuration(target.transform.position, airborneDuration);
        Instantiate(hit, target.transform.position, Quaternion.Euler((target.transform.position + target.GetCenterOffset()) - (info.owner.transform.position + info.owner.GetCenterOffset())), transform).GetComponent<ParticleSystem>().Play();
    }
}
