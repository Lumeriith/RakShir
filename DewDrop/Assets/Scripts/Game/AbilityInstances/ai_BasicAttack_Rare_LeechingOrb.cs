using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_BasicAttack_Rare_LeechingOrb : AbilityInstance
{
    public float travelSpeed = 20f;
    public float manaHealPercentage = 1f;
    public float empoweredManaHealPercentage = 3f;

    private Vector3 targetOffset;
    private ParticleSystem fly;
    private ParticleSystem land;
    private ParticleSystem heal;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        targetOffset = info.target.GetRandomOffset();
        fly = transform.Find<ParticleSystem>("Fly");
        land = transform.Find<ParticleSystem>("Land");
        heal = transform.Find<ParticleSystem>("Heal");
        fly.Play();
        transform.position = info.owner.rightHand.position;
    }

    protected override void AliveUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, info.target.transform.position + targetOffset, travelSpeed * Time.deltaTime);
        if(isMine && Vector3.Distance(transform.position, info.target.transform.position + targetOffset) < 0.5f)
        {
            info.owner.DoBasicAttackImmediately(info.target, this);
            if (info.target.currentHealth <= info.target.maximumHealth / 2f)
            {
                info.owner.DoManaHeal(info.owner, empoweredManaHealPercentage / 100f * info.owner.stat.finalMaximumMana, true, this);
            }
            else
            {
                info.owner.DoManaHeal(info.owner, manaHealPercentage / 100f * info.owner.stat.finalMaximumMana, true, this);
            }
            SFXManager.CreateSFXInstance("si_BasicAttack_Rare_LeechingOrb Hit", transform.position);

            photonView.RPC("RpcHit", RpcTarget.All);
            Despawn();
        }
    }

    [PunRPC]
    private void RpcHit()
    {
        fly.Stop();
        land.Play();
        heal.transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        heal.transform.parent = info.owner.transform;
        heal.gameObject.AddComponent<AutoDestroyPS>();
        heal.Play();
    }
}
