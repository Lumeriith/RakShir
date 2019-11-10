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
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true));
            info.owner.LookAt(transform.position + info.directionVector, true);
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
