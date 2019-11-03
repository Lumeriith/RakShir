using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_HamstringBuff : AbilityInstance
{
    private StatusEffect speed;
    public float speedAmount;
    public float speedDuration;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        if (photonView.IsMine)
        {
            speed = StatusEffect.Speed(info.owner, speedDuration, speedAmount);
            info.owner.ApplyStatusEffect(speed);
            info.owner.control.skillSet[0].ResetCooldown();
        }
    }

    protected override void OnReceiveEvent(string eventString)
    {
        if(eventString == "RemoveBuff")
        {
            if (photonView.IsMine && speed.isAlive) speed.Remove();
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
