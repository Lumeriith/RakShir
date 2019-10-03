using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Default_FlyingKick : AbilityInstance
{
    private CastInfo info;
    public float distance;
    public float speed;

    public float airborneDistance;
    public float airborneTime;

    public float bonusDamage;
    public TargetValidator targetValidator;

    public SelfValidator channelValidator;

    private ParticleSystem fly;
    private ParticleSystem hit;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        fly.Play();
        if (!photonView.IsMine) return;
        info = castInfo;

        Vector3 targetPosition = info.owner.transform.position + info.directionVector * distance;
        float dashDuration = Vector3.Distance(targetPosition, info.owner.transform.position) / speed;
        info.owner.DashThroughForDuration(targetPosition, dashDuration);
        Channel channel = new Channel(channelValidator, dashDuration, false, false, true, false, null, Stopped);
        info.owner.control.StartChanneling(channel);
        StartCoroutine(CoroutineEndAfterFullDistance(dashDuration));
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

        info.owner.CancelDash();

        photonView.RPC("RpcHit", RpcTarget.All, other.ClosestPoint(transform.position));
        info.owner.DoBasicAttackImmediately(lv);
        info.owner.DoMagicDamage(bonusDamage, lv);

        lv.AirborneForDuration(lv.transform.position + info.owner.transform.forward * airborneDistance, airborneTime);

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }


    [PunRPC]
    private void RpcHit(Vector3 position)
    {
        fly.Stop();
        hit.transform.position = position;
        hit.Play();
    }





    IEnumerator CoroutineEndAfterFullDistance(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!isAlive) yield break;
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();

    }
}
