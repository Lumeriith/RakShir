using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_Charge : AbilityInstance
{
    public SelfValidator selfValidator;
    private ParticleSystem hit;
    private ParticleSystem charge;
    private new Collider collider;
    private Vector3 chargeDestination;

    public TargetValidator targetValidator;

    public float chargeDelay = .5f;

    public float chargeSpeed = 10f;
    public float airborneDistance = 1f;
    public float airborneDuration = 0.5f;
    public float damage = 50f;
    public float slowAmount = 30f;
    public float slowDuration = 2f;
    public float radius = 2.5f;
    public float shieldAmount = 30f;
    public float shieldDuration = 5f;

    private Vector3 forwardVector;
    private Displacement displacement;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
        charge = transform.Find("Charge").GetComponent<ParticleSystem>();
        collider = GetComponent<Collider>();
        collider.enabled = false;
        chargeDestination = castInfo.target.transform.position;
        transform.LookAt(chargeDestination);
        forwardVector = transform.forward;
        forwardVector.y = 0f;
        forwardVector.Normalize();
        if (photonView.IsMine)
        {
            info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, ChannelFinished, ChannelCanceled));
        }
    }

    private void ChannelFinished()
    {
        float duration = Vector3.Distance(chargeDestination, info.owner.transform.position) / chargeSpeed;
        photonView.RPC("RpcCharge", RpcTarget.All);
        displacement = new Displacement(chargeDestination - info.owner.transform.position, duration, true, true, EasingFunction.Ease.Linear, StopCharge, StopCharge);
        info.owner.StartDisplacement(displacement);
        collider.enabled = true;
        info.owner.PlayCustomAnimation("Rare - Charge");
    }

    private void StopCharge()
    {
        photonView.RPC("RpcStopCharge", RpcTarget.All);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive) return;
        if (!photonView.IsMine) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        displacement.Cancel();
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(other.transform.position, radius, targetValidator);

        for (int i = 0;i < targets.Count; i++)
        {
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
            info.owner.DoMagicDamage(damage, targets[i]);
            targets[i].StartDisplacement(new Displacement(forwardVector * airborneDistance, airborneDuration, false, false, EasingFunction.Ease.EaseOutQuad));
            targets[i].ApplyStatusEffect(StatusEffect.Slow(info.owner, slowDuration, slowAmount));
        }

        if (targets.Count != 0)
        {
            info.owner.ApplyStatusEffect(StatusEffect.Shield(info.owner, shieldDuration, shieldAmount * targets.Count));
        }

        photonView.RPC("RpcStopCharge", RpcTarget.All);

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcStopCharge()
    {
        if (!isAlive) return;
        charge.Stop();
    }


    [PunRPC]
    private void RpcCharge()
    {
        if (!isAlive) return;
        charge.Play();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        if (!isAlive) return;
        LivingThing target = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit.gameObject, target.transform.position + target.GetCenterOffset(), Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }

    private void ChannelCanceled()
    {
        if (!isAlive) return;
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;

    }
}
