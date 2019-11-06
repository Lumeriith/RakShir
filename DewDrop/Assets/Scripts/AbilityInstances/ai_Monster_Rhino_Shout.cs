using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Rhino_Shout : AbilityInstance
{

    public TargetValidator targetValidator;
    public float range;
    public float airborneDuration = 0.65f;
    public float airborneDistance = 1.5f;
    public float damage;


    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        List<LivingThing> lvs = info.owner.GetAllTargetsInRange(info.owner.transform.position, range, targetValidator);
        for(int i = 0; i < lvs.Count; i++)
        {
            lvs[i].AirborneForDuration(lvs[i].transform.position + (lvs[i].transform.position - info.owner.transform.position).normalized * airborneDistance, airborneDuration);
            info.owner.DoMagicDamage(damage, lvs[i]);
        }
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
