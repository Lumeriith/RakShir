using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_HealOverTime : AbilityInstance
{
    private float duration;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;

        info.owner.statusEffect.ApplyStatusEffect(StatusEffect.HealOverTime(source, (float)data[1], (float)data[0], true));
        duration = (float)data[1];
        StartCoroutine(CoroutineDestroy());
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
    }

    IEnumerator CoroutineDestroy()
    {
        yield return new WaitForSeconds(duration);
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }
}
