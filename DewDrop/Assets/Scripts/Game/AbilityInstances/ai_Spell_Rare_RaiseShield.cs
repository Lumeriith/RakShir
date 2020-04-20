using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_RaiseShield : AbilityInstance
{
    public SelfValidator channelValidator;
    public TargetValidator targetValidator;
    public float duration = 1.25f;
    public float speed = 20f;
    public float counterDistance = 2.2f;

    public float pushDistance = 0.5f;
    public float pushDuration = 0.25f;

    public float stunDuration = 0.75f;

    private Vector3 startPosition;

    private ParticleSystem start;
    private ParticleSystem counter;
    private GameObject hit;

    private StatusEffect protection;
    private Channel channel;

    private new Collider collider;

    private bool isActivated = false;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        counter = transform.Find("Counter").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").gameObject;
        collider = GetComponent<Collider>();
        collider.enabled = false;
        startPosition = transform.position;

        start.Play();
        if (!photonView.IsMine) return;
        protection = StatusEffect.Protected(source, duration);
        info.owner.ApplyStatusEffect(protection);
        channel = new Channel(channelValidator, duration, false, false, false, false, Stop, Stop);
        info.owner.control.StartChanneling(channel);
        info.owner.OnTakeDamage += TookDamage;
    }

    private void TookDamage(InfoDamage info)
    {
        this.info.owner.OnTakeDamage -= TookDamage;
        photonView.RPC("RpcActivate", RpcTarget.All);
        protection.Remove();
        channel.Cancel();
        collider.enabled = true;
        this.info.owner.PlayCustomAnimation("Rare - SwordAndShield RaiseShield Attack", 0.8f);
    }

    [PunRPC]
    private void RpcActivate()
    {
        isActivated = true;
        counter.Play();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated || !photonView.IsMine) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        info.owner.DoBasicAttackImmediately(thing, source);
        thing.StartDisplacement(Displacement.ByVector(info.directionVector * pushDistance, pushDuration, false, false, false, Ease.EaseOutQuad, null, null));
        thing.ApplyStatusEffect(StatusEffect.Stun(source, stunDuration));
        photonView.RPC("RpcHit", RpcTarget.All, thing.photonView.ViewID);
    }
    protected override void AliveUpdate()
    {
        if (!isActivated) return;
        
        if(Vector3.Distance(transform.position, startPosition) < counterDistance)
        {
            transform.position += info.directionVector * speed * Time.deltaTime;
        } else if (photonView.IsMine)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }

    [PunRPC]
    private void RpcHit(int viewId)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewId).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }


    private void Stop()
    {
        if (protection.isAlive)
        {
            protection.Remove();
            info.owner.OnTakeDamage -= TookDamage;
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmittingAndClear);
            DestroySelf();
        }
        else if (!isActivated)
        {
            info.owner.OnTakeDamage -= TookDamage;
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.DontStop);
            DestroySelf();
        }
        
        
    }
}
