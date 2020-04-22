using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_BasicAttack_Rare_PainOrb : AbilityInstance
{
    public float projectileSpeed = 10f;
    public float bonusDamage = 20f;
    public float slowAmount = 20f;
    public float slowDuration = 2f;
    public float minModelScale = 0.9f;
    public float maxModelScale = 1.2f;

    private ParticleSystem land;
    private ParticleSystem fly;
    private ParticleSystem empoweredLand;
    private Transform model;

    private Vector3 offset;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.position = info.owner.rightHand.position;
        offset = info.target.GetRandomOffset();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        model = transform.Find("Model");
        empoweredLand = transform.Find("EmpoweredLand").GetComponent<ParticleSystem>();
        fly.Play();
    }

    protected override void AliveUpdate()
    {
        model.localScale = Vector3.one * Random.Range(minModelScale, maxModelScale);    
        transform.position = Vector3.MoveTowards(transform.position, info.target.transform.position + offset, projectileSpeed * Time.deltaTime);
        if (photonView.IsMine && transform.position == info.target.transform.position + offset)
        {
            info.owner.DoBasicAttackImmediately(info.target, source);
            List<StatusEffect> statusEffects = info.target.statusEffect.GetCustomStatusEffectsByName("고통");
            if (statusEffects.Count != 0)
            {
                info.target.ApplyStatusEffect(StatusEffect.Slow(source, slowDuration, slowAmount));
                info.owner.DoPureDamage(bonusDamage, info.target, source);
                photonView.RPC("RpcLanded", RpcTarget.All, true);
                for(int i = 0; i < statusEffects.Count; i++)
                {
                    statusEffects[i].Remove();
                }

                SFXManager.CreateSFXInstance("si_BasicAttack_Rare_PainOrb EmpoweredHit", transform.position);

            }
            else
            {
                SFXManager.CreateSFXInstance("si_BasicAttack_Rare_PainOrb Hit", transform.position);
                photonView.RPC("RpcLanded", RpcTarget.All, false);
            }
            
            DetachChildParticleSystemsAndAutoDelete();
            Despawn();
        }
    }

    [PunRPC]
    private void RpcLanded(bool empowered)
    {
        if (empowered) empoweredLand.Play();
        land.Play();
        fly.Stop();
    }
}
