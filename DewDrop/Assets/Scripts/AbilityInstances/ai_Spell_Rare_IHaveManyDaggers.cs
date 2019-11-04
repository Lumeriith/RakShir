using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_IHaveManyDaggers : AbilityInstance
{
    public float cooldownReductionMultiplier = 7f;
    public float duration = 8f;

    private float startTime;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
        if (!photonView.IsMine) return;
        startTime = Time.time;
    }

    protected override void AliveUpdate()
    {
        if(photonView.IsMine && info.owner.control.skillSet[1] != null && info.owner.control.skillSet[1] as trg_Spell_Rare_ThrowDagger != null)
        {
            info.owner.control.skillSet[1].ApplyCooldownReduction(Time.deltaTime * cooldownReductionMultiplier);
        }

        if (photonView.IsMine && Time.time - startTime > duration)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
