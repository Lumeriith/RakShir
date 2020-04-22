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

    public TargetValidator targetValidator;

    public float chargeSpeed = 10f;
    public float airborneDistance = 1f;
    public float airborneDuration = 0.5f;
    public float damage = 50f;
    public float stunDuration = 1.5f;
    public float radius = 2.5f;

    

    private SFXInstance chargeSFX;
    private Vector3 forwardVector;
    private Displacement displacement;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
        charge = transform.Find("Charge").GetComponent<ParticleSystem>();
        collider = GetComponent<Collider>();
        collider.enabled = false;
        photonView.RPC("RpcCharge", RpcTarget.All);
        chargeSFX = SFXManager.CreateSFXInstance("si_Spell_Rare_Charge", transform.position);
        chargeSFX.Follow(this);
        displacement = Displacement.TowardsTarget(info.target, 0.5f, chargeSpeed, true, true, ChargeFinished, StopCharge);
        info.owner.StartDisplacement(displacement);
        collider.enabled = true;
        info.owner.PlayCustomAnimation("Rare - Charge");
    }



    private void StopCharge()
    {
        photonView.RPC("RpcStopCharge", RpcTarget.All);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }

    private void ChargeFinished()
    {
        if (!isAlive) return;
        if (!photonView.IsMine) return;
        if (info.target == null || !targetValidator.Evaluate(info.owner, info.target)) return;
        SFXManager.CreateSFXInstance("si_Spell_Rare_Charge Hit", transform.position);
        displacement.Cancel();
        //chargeSFX.Stop();
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(info.target.transform.position, radius, targetValidator);

        for (int i = 0; i < targets.Count; i++)
        {
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
            info.owner.DoMagicDamage(damage, targets[i], false, source);
            targets[i].StartDisplacement(Displacement.ByVector(info.owner.transform.forward * airborneDistance, airborneDuration, false, false, false, Ease.EaseOutQuad));
            targets[i].ApplyStatusEffect(StatusEffect.Stun(source, stunDuration));
        }


        photonView.RPC("RpcStopCharge", RpcTarget.All);

        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
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

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
        transform.rotation = info.owner.transform.rotation;
    }
}
