using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Elemental_DoubleKick : AbilityInstance
{
    public SelfValidator channelValidator;

    public float marginToTarget;
    public float dashSpeed;

    public float bonusDamage;

    public float kickDelay = 0.2f;
    public float kickInterval = 0.3f;
    public float afterKickDelay = 0.2f;

    private GameObject hitEffect;

    private LivingThing target;

    private void Awake()
    {
        hitEffect = transform.Find("HitEffect").gameObject;
    }


    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;

        target = PhotonNetwork.GetPhotonView((int)data[0]).GetComponent<LivingThing>();

        info.owner.StartDisplacement(new Displacement(target, marginToTarget, dashSpeed, true, true, DashFinished, DashCanceled));

    }
    private void DashFinished()
    {
        StartCoroutine(CoroutineKicks());
    }

    private bool isKicking = false;
    private IEnumerator CoroutineKicks()
    {
        isKicking = true;
        info.owner.PlayCustomAnimation("Elemental - W Double Kick", 1f);
        info.owner.control.StartChanneling(new Channel(channelValidator, kickInterval + afterKickDelay, false, false, false, false, null, () =>
        {
            if (isKicking)
            {
                StopCoroutine("CoroutineKicks");
                DashCanceled();
            }
        }));
        yield return new WaitForSeconds(kickDelay);
        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        Kick();
        yield return new WaitForSeconds(kickInterval);
        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        Kick();
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    private void DashCanceled()
    {
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

    private void NoTarget()
    {
        info.owner.control.skillSet[2].ResetCooldown();
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
