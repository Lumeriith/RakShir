using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Elemental_FlyingKick : AbilityInstance
{
    public float distance;
    public float speed;

    public float airborneDistance;
    public float airborneTime;

    public float bonusDamage;
    public TargetValidator targetValidator;

    public SelfValidator channelValidator;

    private ParticleSystem fly;
    private ParticleSystem hit;

    private SFXInstance flyingSound;

    private Displacement displacement;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        fly.Play();
        if (!photonView.IsMine) return;
        flyingSound = SFXManager.CreateSFXInstance("si_Spell_Elemental_FlyingKick", transform.position);
        flyingSound.Follow(this);
        displacement = Displacement.ByVector(info.directionVector * distance, (info.directionVector * distance).magnitude / speed, true, true, false, Ease.EaseOutSine, Stopped, Stopped);
        info.owner.StartDisplacement(displacement);
    }

    private void Stopped()
    {
        if (!isAlive) return;
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        transform.rotation = info.owner.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive || !photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null) return;
        if (!targetValidator.Evaluate(info.owner, lv)) return;

     
        if(flyingSound != null) flyingSound.Stop();
        photonView.RPC("RpcHit", RpcTarget.All, other.ClosestPoint(transform.position));
        info.owner.DoBasicAttackImmediately(lv, source);
        info.owner.DoMagicDamage(bonusDamage, lv, false, source);
        SFXManager.CreateSFXInstance("si_Spell_Elemental_FlyingKick Hit", transform.position);
        lv.StartDisplacement(Displacement.ByVector(info.owner.transform.forward * airborneDistance, airborneTime, false, false, true, Ease.EaseOutSine));

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
        displacement.Cancel();
    }


    [PunRPC]
    private void RpcHit(Vector3 position)
    {
        fly.Stop();
        hit.transform.position = position;
        hit.transform.rotation = info.owner.transform.rotation;
        hit.Play();
    }
}
