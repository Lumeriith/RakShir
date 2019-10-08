using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Test_EzrealQ : AbilityInstance
{

    CastInfo info;

    ParticleSystem fly;
    ParticleSystem land;

    public TargetValidator tv;

    public float speed = 10f;
    public float distance = 10f;
    public float damage = 100f;
    public float stunDuration = 1f;

    Vector3 startPosition;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        fly.Play();
        startPosition = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if(Vector3.Distance(transform.position, startPosition) > distance)
        {
            photonView.RPC("RpcFizzle", RpcTarget.All);
            if (photonView.IsMine)
            {
                DetachChildParticleSystemsAndAutoDelete();
                DestroySelf();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing victim = other.GetComponent<LivingThing>();
        if (victim == null) return;
        if (!tv.Evaluate(info.owner, victim)) return;
        info.owner.DoMagicDamage(damage, victim);

        StatusEffect stun = new StatusEffect(info.owner, StatusEffectType.Stun, stunDuration);
        victim.statusEffect.ApplyStatusEffect(stun);
        photonView.RPC("RpcLand", RpcTarget.All);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcFizzle()
    {
        fly.Stop();
    }

    [PunRPC]
    private void RpcLand()
    {
        fly.Stop();
        land.Play();
        
    }
}
