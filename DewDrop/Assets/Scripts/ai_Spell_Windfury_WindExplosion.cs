using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Windfury_WindExplosion : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;
    private List<LivingThing> targets = new List<LivingThing>();
    private bool done;
    private float timer;

    public float pushDistance;
    public float airborneDuration;
    public float damage;
    public float particleDuration;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        start.Play();
    }

    protected override void AliveUpdate()
    {
        timer += Time.deltaTime;
        foreach (LivingThing target in targets)
        {
            Vector3 directionVector = (target.transform.position + target.GetCenterOffset() - transform.position).normalized;
            target.AirborneForDuration(target.transform.position + directionVector * pushDistance, airborneDuration);
            info.owner.DoMagicDamage(damage, target);
            
        }
        if (timer >= particleDuration)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null || target == info.owner) { return; }
        targets.Add(target);
    }
}
