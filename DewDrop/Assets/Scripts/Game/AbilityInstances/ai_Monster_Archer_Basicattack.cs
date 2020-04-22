using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Monster_Archer_Basicattack : AbilityInstance
{
    public float projectileSpeed = 10f;

    private ParticleSystem land;
    private ParticleSystem fly;

    private Vector3 offset;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        offset = info.target.GetRandomOffset();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        fly.Play();
    }

    protected override void AliveUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, info.target.transform.position + offset, projectileSpeed * Time.deltaTime);
        transform.LookAt(info.target.transform.position + offset);
        if (photonView.IsMine && transform.position == info.target.transform.position + offset)
        {
            info.owner.DoBasicAttackImmediately(info.target, source);
            photonView.RPC("RpcLanded", RpcTarget.All);
            fly.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            DetachChildParticleSystemsAndAutoDelete();
            Despawn();
        }
    }

    [PunRPC]
    private void RpcLanded()
    {
        land.Play();
        fly.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
