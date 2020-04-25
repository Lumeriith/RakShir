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
            photonView.RPC("RpcStopFly", RpcTarget.All);
            
            Despawn();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null || !targetValidator.Evaluate(info.owner, lv)) return;

        if (isEmpowered)
        {
            info.owner.DoMagicDamage(lv, damage + bonusDamage, false, this);
            lv.statusEffect.ApplyStatusEffect(StatusEffect.Stun(stunDuration), this);
            SFXManager.CreateSFXInstance("si_Spell_Rare_MagicArrow EmpoweredHit", transform.position);
        }
        else
        {
            info.owner.DoMagicDamage(lv, damage, false, this);
            SFXManager.CreateSFXInstance("si_Spell_Rare_MagicArrow Hit", transform.position);
        }

        Vector3 position = other.transform.position;
        position.y = transform.position.y;
        photonView.RPC("RpcLanded", RpcTarget.All, position);
    }

    [PunRPC]
    private void RpcLanded(Vector3 position)
    {
        ParticleSystem newLand = Instantiate(land.gameObject, position, transform.rotation, null).GetComponent<ParticleSystem>();
        newLand.Play();
        newLand.gameObject.AddComponent<ParticleSystemAutoDestroy>();
        if (isEmpowered)
        {
            ParticleSystem newEmpoweredLand = Instantiate(empoweredLand.gameObject, position, transform.rotation, null).GetComponent<ParticleSystem>();
            newEmpoweredLand.Play();
            newEmpoweredLand.gameObject.AddComponent<ParticleSystemAutoDestroy>();
        }
        //fly.Stop();
    }

    [PunRPC]
    private void RpcStopFly()
    {
        fly.Stop();
        empoweredFly.Stop();
    }
}
