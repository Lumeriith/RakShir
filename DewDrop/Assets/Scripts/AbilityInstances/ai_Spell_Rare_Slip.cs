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
        StartCoroutine(CoroutineSlip());
    }

    private IEnumerator CoroutineSlip()
    {
        Vector3 posA = transform.position + info.directionVector * distance * 3f / 4f;
        Vector3 posB = transform.position + info.directionVector * distance;
        
        info.owner.stat.bonusDodgeChance += 50f;

        if (photonView.IsMine)
        {
            info.owner.DashThroughForDuration(posA, duration / 2f + .1f);
            info.owner.ApplyStatusEffect(StatusEffect.HealOverTime(info.owner, healDuration, (info.owner.maximumHealth - info.owner.currentHealth) * healMultiplier));
        }
        yield return new WaitForSeconds(duration / 2f);
        if (photonView.IsMine) info.owner.DashThroughForDuration(posB, duration / 2f);
        yield return new WaitForSeconds(duration / 2f);
        info.owner.stat.bonusDodgeChance -= 50f;
        if (photonView.IsMine)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }
}
