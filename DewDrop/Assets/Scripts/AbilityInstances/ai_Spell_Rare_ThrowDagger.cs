using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_ThrowDagger : AbilityInstance
{
    private ParticleSystem land;
    private ParticleSystem fly;
    private Transform model;
    private Vector3 start;

    public TargetValidator targetValidator;
    public float speed = 14f;
    public float distance = 7f;
    public float modelAngularSpeed = 1200f;
    public float damage = 30f;
    public float cooldownReductionAmount = 4f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        model = transform.Find("Model");
        fly.Play();
        start = transform.position;
    }

    protected override void AliveUpdate()
    {
        model.Rotate(model.right, modelAngularSpeed * Time.deltaTime, Space.World);
        transform.position += transform.forward * speed * Time.deltaTime;
        if (photonView.IsMine)
        {
            if (Vector3.Distance(transform.position, start) >= distance)
            {
                photonView.RPC("RpcDestroyModel", RpcTarget.All);
                DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
                DestroySelf();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null || !targetValidator.Evaluate(info.owner, lv)) return;
        SFXManager.CreateSFXInstance("si_Spell_Rare_ThrowDagger Hit", transform.position);
        info.owner.DoBasicAttackImmediately(lv);
        info.owner.DoMagicDamage(30f, lv);
        photonView.RPC("RpcLand", RpcTarget.All, transform.position);
        photonView.RPC("RpcDestroyModel", RpcTarget.All);
        if (info.owner.control.skillSet[1] != null && info.owner.control.skillSet[1] as trg_Spell_Rare_ThrowDagger != null)
        {
            info.owner.control.skillSet[1].ApplyCooldownReduction(cooldownReductionAmount);
            
        }
        info.owner.DoManaHeal(20f, info.owner, true);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
    [PunRPC]
    private void RpcDestroyModel()
    {
        Destroy(model.gameObject);
    }

    [PunRPC]
    private void RpcLand(Vector3 position) {
        transform.position = position;
        fly.Stop();
        land.Play();
    }
}
