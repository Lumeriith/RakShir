using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_IceBolt : AbilityInstance
{
    public Vector3 offset;

    public float speed = 8f;
    public float distance = 8f;

    public float damage = 60f;
    public float slowAmount = 40f;
    public float slowDuration = 3f;
    public float slowedEnemiesBonusMultiplier = 1.5f;

    public TargetValidator validator;

    private ParticleSystem start;
    private ParticleSystem hit;

    private Vector3 startPosition;


    protected override void OnCreate(CastInfo info, object[] data)
    {
        start = transform.Find<ParticleSystem>("Start");
        hit = transform.Find<ParticleSystem>("Hit");
        start.Play();
        transform.position += transform.rotation * offset;
        startPosition = transform.position;
        if (isMine) SFXManager.CreateSFXInstance("si_Spell_Rare_IceBolt Spawn", transform.position);
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if(isMine && Vector3.Distance(transform.position, startPosition) > distance)
        {
            Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine) return;
        Entity thing = other.GetComponent<Entity>();
        if (thing == null || !validator.Evaluate(info.owner, thing)) return;
        if (thing.IsAffectedBy(StatusEffectType.Slow))
        {
            info.owner.DoMagicDamage(thing, damage * slowedEnemiesBonusMultiplier, false, this);
        }
        else
        {
            info.owner.DoMagicDamage(thing, damage, false, this);
        }
        SFXManager.CreateSFXInstance("si_Spell_Rare_IceBolt Hit", transform.position);
        thing.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), this);
        photonView.RPC("RpcHit", RpcTarget.All);
        Despawn(DespawnBehaviour.WaitForParticleSystems);
    }

    [PunRPC]
    private void RpcHit()
    {
        start.Stop();
        hit.Play();
    }

}
