using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class spl_ConfettiProjectile : AbilityInstance
{
    LivingThing target;
    LivingThing attacker;

    public float projectileSpeed = 15;

    private ParticleSystem ps_main;
    private ParticleSystem ps_start;
    private ParticleSystem ps_end;
    private ParticleSystem ps_projectile;

    private bool hasLanded = false;

    protected override void OnCreate(AbilityInstanceManager.CastInfo castInfo, object[] data)
    {
        target = castInfo.target;
        attacker = castInfo.owner;
        transform.LookAt(castInfo.target.transform);

        ps_main = GetComponent<ParticleSystem>();
        ps_start = transform.Find("Start Particle").GetComponent<ParticleSystem>();
        ps_projectile = transform.Find("Projectile Particle").GetComponent<ParticleSystem>();
        ps_end = transform.Find("End Particle").GetComponent<ParticleSystem>();

        ps_start.Play();
        ps_projectile.Play();
    }



    private void Update()
    {
        if (!isCreated) return;

        if (!hasLanded)
        {
            Vector3 targetLocation = target.transform.position + Vector3.up * 0.5f;
            transform.position = Vector3.MoveTowards(transform.position, targetLocation, projectileSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetLocation) < 0.1)
            {
                hasLanded = true;
                ps_projectile.Clear();
                ps_projectile.Stop();
                ps_end.Play();
                if (photonView.IsMine)
                {
                    attacker.DoBasicAttack(target);

                    CoreStatusEffect stun = new CoreStatusEffect(attacker, CoreStatusEffectType.Stun, 5f);

                    target.statusEffect.ApplyCoreStatusEffect(stun);
                    stun.SetDuration(1f);

                    
                    
                }
            }
        }
        else
        {
            if (!ps_main.IsAlive() && photonView.IsMine)
            {
                PhotonNetwork.Destroy(photonView);
            }
        }

    }
}
