using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_CurrentOfSwiftness : AbilityInstance
{
    private StatusEffect shield;
    private StatusEffect speed;
    public float shieldAmount = 70f;
    public float shieldDuration = 12f;
    public float speedAmount = 20f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        if (!photonView.IsMine) return;
        shield = StatusEffect.Shield(info.owner, shieldDuration, shieldAmount);
        speed = StatusEffect.Speed(info.owner, shieldDuration, speedAmount);
        info.owner.statusEffect.ApplyStatusEffect(shield);
        info.owner.statusEffect.ApplyStatusEffect(speed);
    }


    protected override void AliveUpdate()
    {
        transform.rotation = Quaternion.identity;
        if (!photonView.IsMine) return;
        if (shield == null || !shield.isAlive || speed == null || !speed.isAlive)
        {
            if (speed != null && speed.isAlive) speed.Remove();
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
