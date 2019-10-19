using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_WindBarrier : AbilityInstance
{
    private CastInfo info;

    private ParticleSystem barrierParticle;

    public float duration = 3f;
    public float basicDamageAbsorptionAmount = 100f;
    public float bonusAbsorptionAmountPerMovementSpeed = 0.2f;

    private float timer;

    private void Awake()
    {
        barrierParticle = transform.Find("Start").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        // Add barrier
        barrierParticle.Play();
        timer = 0f;
    }

    protected override void AliveUpdate()
    {
        if (!photonView.IsMine) return;
        timer += Time.deltaTime;
        photonView.RPC("RpcSyncParticle", RpcTarget.All);

        if (timer >= duration)
        {
            DetachChildParticleSystemsAndAutoDelete(ParticleSystemStopBehavior.StopEmittingAndClear);
            DestroySelf();
        }
    }

    [PunRPC]
    private void RpcSyncParticle()
    {
        barrierParticle.transform.position = info.owner.transform.position;
    }
}
