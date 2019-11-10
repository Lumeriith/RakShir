using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Slip : AbilityInstance
{
    public float duration = 1f;
    public float distance = 4.5f;
    public float healDuration = 5f;
    public float healMultiplier = 0.2f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.rotation = castInfo.directionQuaternion;
        Vector3 posA = transform.position + info.directionVector * distance * 3f / 4f;
        Vector3 posB = transform.position + info.directionVector * distance;

        info.owner.stat.bonusDodgeChance += 50f;


        if (photonView.IsMine)
        {
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true, EasingFunction.Ease.EaseOutQuad, StopSlip, StopSlip));
            info.owner.ApplyStatusEffect(StatusEffect.HealOverTime(info.owner, healDuration, (info.owner.maximumHealth - info.owner.currentHealth) * healMultiplier));
        }
    }



    private void StopSlip()
    {
        info.owner.stat.bonusDodgeChance -= 50f;
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
