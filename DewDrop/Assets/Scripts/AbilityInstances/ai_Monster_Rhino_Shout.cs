using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Rhino_Shout : AbilityInstance
{

    public TargetValidator targetValidator;
    public float range;
    public float slowDuration;
    public float slowAmount;
    public float damage;
    public float shieldAmount;
    public float shieldDuration;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        List<LivingThing> lvs = info.owner.GetAllTargetsInRange(info.owner.transform.position, range, targetValidator);
        for(int i = 0; i < lvs.Count; i++)
        {
            lvs[i].statusEffect.ApplyStatusEffect(StatusEffect.Slow(info.owner, slowDuration, slowAmount));
            info.owner.DoMagicDamage(damage, lvs[i]);
        }
        info.owner.statusEffect.ApplyStatusEffect(StatusEffect.Shield(info.owner, shieldDuration, shieldAmount));
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
