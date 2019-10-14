using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Elemental_DoubleKick : AbilityInstance
{
    public SelfValidator channelValidator;

    public float range;
    public float marginToTarget;
    public float dashSpeed;

    public float bonusDamage;

    public float animationPreStartTime;
    public float delayBeforeFirstKick;
    public float delayBeforeSecondKick;
    public float delayAfterSecondKick;


    public TargetValidator targetValidator;

    private GameObject hitEffect;

    private CastInfo info;
    private LivingThing target;

    private void Awake()
    {
        hitEffect = transform.Find("HitEffect").gameObject;
    }


    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info = castInfo;

        target = PhotonNetwork.GetPhotonView((int)data[0]).GetComponent<LivingThing>();

        StartCoroutine("CoroutineDashKick");
 
    }

    private IEnumerator CoroutineDashKick()
    {
        Vector3 targetPosition = target.transform.position - (target.transform.position - info.owner.transform.position).normalized * marginToTarget;
        float dashDuration;
        float dashDistance = Vector3.Distance(info.owner.transform.position, targetPosition);
        dashDuration = dashDistance / dashSpeed;
        Channel channel = new Channel(channelValidator, dashDuration + delayBeforeFirstKick + delayBeforeSecondKick + delayAfterSecondKick, false, false, false, false, null, CancelDashKick);
        info.owner.control.StartChanneling(channel);
        info.owner.DashThroughForDuration(targetPosition, dashDuration);
        info.owner.LookAt(target.transform.position, true);
        yield return new WaitForSeconds(dashDuration - animationPreStartTime);
        if (isDashKickCanceled)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
            yield break;
        }
        info.owner.PlayCustomAnimation("Elemental - W Double Kick", 1f);
        yield return new WaitForSeconds(delayBeforeFirstKick);
        if (isDashKickCanceled)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
            yield break;
        }
        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        Kick();

        yield return new WaitForSeconds(delayBeforeSecondKick);
        if (isDashKickCanceled)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
            yield break;
        }
        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        Kick();

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    private void Kick()
    {
        info.owner.DoBasicAttackImmediately(target);
        info.owner.DoMagicDamage(bonusDamage, target);
        info.owner.LookAt(target.transform.position, true);
    }

    [PunRPC]
    private void CreateHitEffect(Vector3 position)
    {
        Instantiate(hitEffect, position, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }


    private bool isDashKickCanceled = false;

    private void CancelDashKick()
    {
        isDashKickCanceled = true;
    }

    private void NoTarget()
    {
        info.owner.control.skillSet[2].ResetCooldown();
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
