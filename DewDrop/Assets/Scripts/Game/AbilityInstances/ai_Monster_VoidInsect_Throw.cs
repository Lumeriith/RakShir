using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Monster_VoidInsect_Throw : AbilityInstance
{
    public float silenceDuration = 2f;

    public float poisonDuration = 8f;
    public float poisonAmount = 100f;

    private ParticleSystem fly;
    private ParticleSystem land;
    private Vector3 startPosition;

    public float speed = 8f;
    public float distance = 10f;

    public TargetValidator targetValidator;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        fly.Play();
        startPosition = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (!photonView.IsMine) return;
        if (Vector3.Distance(startPosition, transform.position) > distance)
        {
            Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        Entity lv = other.GetComponent<Entity>();
        if (lv == null || !targetValidator.Evaluate(info.owner, lv)) return;

        lv.ApplyStatusEffect(StatusEffect.DamageOverTime(poisonDuration, poisonAmount), this);
        lv.ApplyStatusEffect(StatusEffect.Silence(silenceDuration), this);

        photonView.RPC("RpcLanded", RpcTarget.All);
        
        Despawn();
    }

    [PunRPC]
    private void RpcLanded()
    {
        land.Play();
        fly.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

}
