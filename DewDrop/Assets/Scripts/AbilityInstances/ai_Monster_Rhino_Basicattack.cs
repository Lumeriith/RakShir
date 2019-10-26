using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Rhino_Basicattack : AbilityInstance
{
    public float distance = 1.5f;
    public float duration = 0.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.Find("Flash").position = castInfo.target.transform.position + castInfo.target.GetCenterOffset();

        info.owner.DashThroughForDuration(info.owner.transform.position + (info.target.transform.position - info.owner.transform.position).normalized * distance, duration);
        if (!photonView.IsMine) return;
        castInfo.owner.DoBasicAttackImmediately(castInfo.target);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    
}
