using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_HolyLight : AbilityInstance
{
    public TargetValidator targetValidator;
    public float healAmount = 50f;
    public float healPerHitAmount = 20f;
    public float damage = 30f;
    public float blindDuration = 2f;
    public float radius = 3f;

    private ParticleSystem flash;
    private GameObject hit;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {

        flash = transform.Find("Flash").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").gameObject;
        flash.Play();
        if (!photonView.IsMine) return;
        float healAmount = this.healAmount;
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].ApplyStatusEffect(StatusEffect.Blind(source, blindDuration));
            info.owner.DoMagicDamage(damage, targets[i], false, source);
            healAmount += healPerHitAmount;
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
        }
        info.owner.DoHeal(healAmount, info.owner, false, source);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, transform);
    }
}
