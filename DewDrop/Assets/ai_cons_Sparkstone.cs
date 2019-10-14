using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_Sparkstone : AbilityInstance
{

    public float delay;

    private ParticleSystem pre;
    private ParticleSystem spark;


    private void Awake()
    {
        pre = transform.Find("Pre").GetComponent<ParticleSystem>();
        spark = transform.Find("Spark").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        pre.Play();
        StartCoroutine(CoroutineSpark());
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
    }

    IEnumerator CoroutineSpark()
    {
        yield return new WaitForSeconds(delay);
        spark.transform.position = info.point + info.owner.GetCenterOffset();
        spark.Play();
        pre.Stop();
        yield return new WaitForEndOfFrame();
        if (photonView.IsMine) info.owner.Teleport(info.point);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
