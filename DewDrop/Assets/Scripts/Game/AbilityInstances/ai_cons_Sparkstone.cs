using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_cons_Sparkstone : AbilityInstance
{

    public float delay;

    private ParticleSystem pre;
    private ParticleSystem spark;
    private ParticleSystem destination;

    private void Awake()
    {
        pre = transform.Find("Pre").GetComponent<ParticleSystem>();
        spark = transform.Find("Spark").GetComponent<ParticleSystem>();
        destination = transform.Find("Destination").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        pre.Play();
        destination.Play();
        destination.transform.position = info.point;
        StartCoroutine(CoroutineSpark());
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        destination.transform.position = info.point;
    }

    IEnumerator CoroutineSpark()
    {
        yield return new WaitForSeconds(delay);
        spark.transform.position = info.point + info.owner.GetCenterOffset();

        if (photonView.IsMine)
        {
            info.owner.Teleport(info.point);
            photonView.RPC("RpcBoom", RpcTarget.All);
            DetachChildParticleSystemsAndAutoDelete(DespawnBehaviour.WaitForParticleSystems);
            Despawn();
        }
    }

    [PunRPC]
    private void RpcBoom()
    {
        spark.Play();
        pre.Stop();
        destination.Stop();
    }
}
