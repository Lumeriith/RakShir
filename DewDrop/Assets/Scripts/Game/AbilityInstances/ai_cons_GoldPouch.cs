using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_GoldPouch : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if(photonView.IsMine) info.owner.EarnGold((float)data[0]);
        StartCoroutine(CoroutineFollow());
    }


    IEnumerator CoroutineFollow()
    {
        float start = Time.time;
        while (Time.time - start < 3f)
        {
            transform.position = info.owner.transform.position;
            yield return null;
        }
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }
}
