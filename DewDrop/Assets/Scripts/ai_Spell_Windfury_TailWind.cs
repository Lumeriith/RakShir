using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_TailWind : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;

    public float duration = 3f;
    public float bonusMovementSpeedPer = 0.8f;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        info.owner.stat.bonusMovementSpeed += info.owner.stat.baseMovementSpeed * bonusMovementSpeedPer;
        StatusEffect untargetable = new StatusEffect(info.owner, StatusEffectType.Untargetable, duration);
        info.owner.statusEffect.ApplyStatusEffect(untargetable);
        StatusEffect speed = new StatusEffect(info.owner, StatusEffectType.Speed, duration);
        info.owner.statusEffect.ApplyStatusEffect(speed);
        start.Play();
    }

    protected override void AliveUpdate()
    {
        if (!photonView.IsMine) return;

        duration -= Time.deltaTime;

        if (duration > 0)
        {
            photonView.RPC("RpcSyncParticle", RpcTarget.All);
        }
        else
        {
            info.owner.stat.bonusMovementSpeed -= info.owner.stat.baseMovementSpeed * bonusMovementSpeedPer;
            DetachChildParticleSystemsAndAutoDelete(ParticleSystemStopBehavior.StopEmittingAndClear);
            DestroySelf();
        }
    }

    [PunRPC]
    private void RpcSyncParticle()
    {
        transform.position = info.owner.transform.position;
    }
}
