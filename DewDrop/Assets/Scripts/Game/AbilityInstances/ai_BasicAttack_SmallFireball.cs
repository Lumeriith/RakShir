using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_BasicAttack_SmallFireball : AbilityInstance
{
    private LivingThing target;
    private LivingThing owner;

    private Vector3 offset;

    private ParticleSystem fly;
    private ParticleSystem land;
    
    public float projectileSpeed;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();

        land.Stop();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        target = castInfo.target;
        owner = castInfo.owner;

        offset = castInfo.target.GetRandomOffset();
    }

    protected override void AliveUpdate()
    {
        Vector3 targetPosition = target.transform.position + offset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, projectileSpeed * Time.deltaTime);
        if (photonView.IsMine)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
            {
                photonView.RPC("Landed", RpcTarget.All);
                owner.DoBasicAttackImmediately(target, reference);
                Despawn();
            }
        }
    }

    [PunRPC]
    private void Landed()
    {
        fly.Stop();
        land.Play();
    }



}
