using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_WindBarrier : AbilityInstance
{
    private CastInfo info;

    private ParticleSystem barrierParticle;
    private LivingThingStat ownerStat;

    public float duration = 3f;
    public float damageAbsorptionAmount = 100f;

    private float timer;

    private void Awake()
    {
        barrierParticle = transform.Find("Start").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        ownerStat = castInfo.owner.GetComponent<LivingThingStat>();
        // Add barrier
        barrierParticle.Play();
        timer = 0f;
    }

    protected override void AliveUpdate()
    {
        timer += Time.deltaTime;
        barrierParticle.transform.position = info.owner.GetCenterOffset() + info.owner.transform.position;

        if (timer >= duration)
        {
            if (photonView.IsMine)
            {
                DetachChildParticleSystemsAndAutoDelete(ParticleSystemStopBehavior.StopEmittingAndClear);
                DestroySelf();
            }
        }
    }
}
