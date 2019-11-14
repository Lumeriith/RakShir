using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Slip : AbilityInstance
{
    public float duration = 1f;
    public float distance = 4.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.rotation = castInfo.directionQuaternion;
        Vector3 posA = transform.position + info.directionVector * distance * 3f / 4f;
        Vector3 posB = transform.position + info.directionVector * distance;

        


        if (photonView.IsMine)
        {
            info.owner.stat.bonusDodgeChance += 75f;
            info.owner.stat.SyncTemporaryAttributes();
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true, EasingFunction.Ease.EaseOutQuad, StopSlip, StopSlip));
        }
    }



    private void StopSlip()
    {
        info.owner.stat.bonusDodgeChance -= 75f;
        info.owner.stat.SyncTemporaryAttributes();
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
