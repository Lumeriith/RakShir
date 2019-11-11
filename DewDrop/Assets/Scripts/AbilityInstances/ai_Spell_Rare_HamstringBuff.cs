using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_HamstringBuff : AbilityInstance
{
    private StatusEffect speed;
    private StatusEffect haste;
    public float speedAmount;
    public float duration = 3f;
    public float hasteAmount = 100f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        if (photonView.IsMine)
        {
            speed = StatusEffect.Speed(info.owner, duration, speedAmount);
            haste = StatusEffect.Haste(info.owner, duration, hasteAmount);
            info.owner.ApplyStatusEffect(speed);
            info.owner.ApplyStatusEffect(haste);
            info.owner.control.skillSet[0].ResetCooldown();
        }
    }

    protected override void OnReceiveEvent(string eventString)
    {
        if(eventString == "RemoveBuff")
        {
            if (photonView.IsMine && speed.isAlive) speed.Remove();
            if (photonView.IsMine && haste.isAlive) haste.Remove();
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
