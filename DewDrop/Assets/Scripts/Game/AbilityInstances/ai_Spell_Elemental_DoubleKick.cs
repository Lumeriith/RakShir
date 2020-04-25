using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Elemental_DoubleKick : AbilityInstance
{
    public SelfValidator channelValidator;
    public TargetValidator targetValidator;

    public float marginToTarget;
    public float dashSpeed;

    public float bonusDamage;

    public float kickDelay = 0.2f;
    public float kickInterval = 0.3f;
    public float afterKickDelay = 0.2f;

    private GameObject hitEffect;

    private Entity target;




    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hitEffect = transform.Find("HitEffect").gameObject;
        if (!photonView.IsMine) return;

        target = PhotonNetwork.GetPhotonView((int)data[0]).GetComponent<Entity>();

        info.owner.StartDisplacement(Displacement.TowardsTarget(target, marginToTarget, dashSpeed, true, true, DashFinished, DashCanceled));
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

        if(!targetValidator.Evaluate(info.owner, target))
        {
            Despawn();
            yield break;
        }

        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        SFXManager.CreateSFXInstance("si_Spell_Elemental_DoubleKick Hit 0", target.transform.position);
        Kick();
        yield return new WaitForSeconds(kickInterval);

        if (!targetValidator.Evaluate(info.owner, target))
        {
            Despawn();
            yield break;
        }

        photonView.RPC("CreateHitEffect", RpcTarget.All, target.transform.position + target.GetCenterOffset());
        SFXManager.CreateSFXInstance("si_Spell_Elemental_DoubleKick Hit 1", target.transform.position);
        Kick();
        Despawn();
    }

    private void DashCanceled()
    {
        Despawn();
    }

    private void Kick()
    {
        info.owner.DoBasicAttackImmediately(target, this);
        info.owner.DoMagicDamage(target, bonusDamage, false, this);
        info.owner.LookAt(target.transform.position, true);
    }

    [PunRPC]
    private void CreateHitEffect(Vector3 position)
    {
        Instantiate(hitEffect, position, Quaternion.LookRotation(target.transform.position - info.owner.transform.position, Vector3.up), transform).GetComponent<ParticleSystem>().Play();
    }

    private void NoTarget()
    {
        info.owner.control.skillSet[2].ResetCooldown();
        Despawn();
    }

}
