using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_JoyOfHunting : AbilityInstance
{
    public float speedAmount = 15f;
    public float hasteAmount = 30f;
    public float duration = 4f;

    private float startTime;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
        if (!photonView.IsMine) return;
        info.owner.ApplyStatusEffect(StatusEffect.Speed(info.owner, duration, speedAmount));
        info.owner.ApplyStatusEffect(StatusEffect.Haste(info.owner, duration, hasteAmount));
        startTime = Time.time;
        
    }

    protected override void AliveUpdate()
    {
        if(photonView.IsMine && Time.time - startTime > duration)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
