using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_ManaHeal : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;

        info.owner.DoManaHeal((float)data[0], info.owner, true, source);
        StartCoroutine(CoroutineDestroy());
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
    }

    IEnumerator CoroutineDestroy()
    {
        yield return new WaitForSeconds(3f);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
