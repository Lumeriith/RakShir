using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_MagicArrow : AbilityInstance
{
    private ParticleSystem fly;
    private ParticleSystem land;

    private ParticleSystem empoweredFly;
    private ParticleSystem empoweredLand;

    private bool isEmpowered = false;

    private Vector3 startPosition;

    public float speed = 25f;
    public float damage = 60f;

    public float stunDuration = 1f;
    public float bonusDamage = 30f;


    public float distance;

    public TargetValidator targetValidator;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        isEmpowered = (bool)data[0];
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        empoweredFly = transform.Find("EmpoweredFly").GetComponent<ParticleSystem>();
        empoweredLand = transform.Find("EmpoweredLand").GetComponent<ParticleSystem>();
        fly.Play();
        if (isEmpowered) empoweredFly.Play();
        startPosition = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (!photonView.IsMine) return;
        if (Vector3.Distance(startPosition, transform.position) > distance)
        {
            fly.Stop();
            empoweredFly.Stop();
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null || !targetValidator.Evaluate(info.owner, lv)) return;

        if (isEmpowered)
        {
            info.owner.DoMagicDamage(damage + bonusDamage, lv);
            lv.statusEffect.ApplyStatusEffect(StatusEffect.Stun(info.owner, stunDuration));
        }
        else
        {
            info.owner.DoMagicDamage(damage, lv);
        }


        photonView.RPC("RpcLanded", RpcTarget.All);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcLanded()
    {
        land.Play();
        if (isEmpowered) empoweredLand.Play();
        fly.Stop();
    }
}
