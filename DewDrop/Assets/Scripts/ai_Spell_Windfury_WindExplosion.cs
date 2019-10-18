using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_WindExplosion : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem Explosion;
    private List<LivingThing> targets = new List<LivingThing>();
    private bool done;
    private float timer;

    public TargetValidator tv;
    public float pushDistance;
    public float airborneDuration;
    public float damage;
    public float range = 3f;
    public float duration = 0.2f;

    private void Awake()
    {
        Explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        Explosion.Play();

        targets = info.owner.GetAllTargetsInRange(info.owner.transform.position, range, tv);
    }

    protected override void AliveUpdate()
    {
        if(!photonView.IsMine) { return; }

        timer += Time.deltaTime;
        foreach (LivingThing target in targets)
        {
            Vector3 directionVector = (target.transform.position + target.GetCenterOffset() - transform.position).normalized;
            target.AirborneForDuration(target.transform.position + directionVector * pushDistance, airborneDuration);
            info.owner.DoMagicDamage(damage, target);
            
        }
        if (timer >= duration)
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
