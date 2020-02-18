using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_Roll : AbilityInstance
{
    public float distance = 3f;
    public float duration = 0.7f;

    public float attackDamageBoostAmount = 50f;
    public float attackDamageBoostDuration = 4f;

    private StatusEffect damageBoost;

    private ParticleSystem start;
    private ParticleSystem hit;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        start = transform.Find<ParticleSystem>("Start");
        hit = transform.Find<ParticleSystem>("Hit");

        start.Play();

        if (photonView.IsMine)
        {
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true));
            info.owner.LookAt(transform.position + info.directionVector, true);
            StartCoroutine("CoroutineApplyDamageBoost");
        }
    }

    private IEnumerator CoroutineApplyDamageBoost()
    {
        yield return new WaitForSeconds(duration);
        damageBoost = StatusEffect.AttackDamageBoost(source, attackDamageBoostDuration, attackDamageBoostAmount);
        info.owner.ApplyStatusEffect(damageBoost);
        damageBoost.OnExpire += EffectExpired;
        info.owner.OnDoBasicAttackHit += EffectUsed;
    }

    private void EffectExpired()
    {
        info.owner.OnDoBasicAttackHit -= EffectUsed;
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    private void EffectUsed(InfoBasicAttackHit info)
    {
        damageBoost.OnExpire -= EffectExpired;
        this.info.owner.OnDoBasicAttackHit -= EffectUsed;
        damageBoost.Remove();
        photonView.RPC("RpcHit", RpcTarget.All, info.to.photonView.ViewID);
        SFXManager.CreateSFXInstance("si_Spell_Rare_Roll Hit", info.to.transform.position);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcHit(int id)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(id).GetComponent<LivingThing>();
        hit.transform.position = thing.transform.position + thing.GetCenterOffset();
        hit.Play();
    }


}
