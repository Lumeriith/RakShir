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
            info.owner.DoBasicAttackImmediately(info.target);
            if (info.target.currentHealth <= info.target.maximumHealth / 2f)
            {
                info.owner.DoManaHeal(empoweredManaHealPercentage / 100f * info.owner.stat.finalMaximumMana, info.owner, true);
            }
            else
            {
                info.owner.DoManaHeal(manaHealPercentage / 100f * info.owner.stat.finalMaximumMana, info.owner, true);
            }
            SFXManager.CreateSFXInstance("si_BasicAttack_Rare_LeechingOrb Hit", transform.position);

            photonView.RPC("RpcHit", RpcTarget.All);
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.DontStop);
            DestroySelf();
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
