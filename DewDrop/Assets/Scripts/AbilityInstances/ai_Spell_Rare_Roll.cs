using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Roll : AbilityInstance
{
    public float distance = 3f;
    public float duration = 0.7f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {

            info.owner.DashThroughForDuration(transform.position + info.directionVector * distance, duration);
            info.owner.LookAt(transform.position + info.directionVector, true);
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
