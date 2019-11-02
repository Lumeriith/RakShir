using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Blink : AbilityInstance
{
    private ParticleSystem start;
    private ParticleSystem end;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        end = transform.Find("End").GetComponent<ParticleSystem>();
        start.transform.position = info.owner.transform.position;
        end.transform.position = info.point;
        start.Play();
        end.Play();
        if (photonView.IsMine)
        {
            info.owner.Teleport(info.point);
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
