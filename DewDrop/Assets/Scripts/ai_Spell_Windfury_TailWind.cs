using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_TailWind : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;

    public float duration = 3f;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        StatusEffect untargetable = new StatusEffect(info.owner, StatusEffectType.Untargetable, duration);
        info.owner.statusEffect.ApplyStatusEffect(untargetable);
        start.Play();
    }

    protected override void AliveUpdate()
    {
        if (!photonView.IsMine) return;

        duration -= Time.deltaTime;

        if (duration > 0)
        {
            transform.position = info.owner.transform.position;
        }
        else
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
