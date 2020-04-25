using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Gem_Rare_Dawn : AbilityInstance
{
    private gem_Rare_Dawn dawn;

    private ParticleSystem circle;
    private ParticleSystem explosion;

    private SFXInstance start;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        dawn = (gem_Rare_Dawn)gem;
        circle = transform.Find<ParticleSystem>("Circle");
        explosion = transform.Find<ParticleSystem>("Explosion");
        circle.Play();
        if(isMine) StartCoroutine("CoroutineDawn");
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }

    private IEnumerator CoroutineDawn()
    {
        start = SFXManager.CreateSFXInstance("si_Gem_Rare_Dawn Start", transform.position);
        start.Follow(this);
        yield return new WaitForSeconds(dawn.explosionDelay);
        start.Stop();
        SFXManager.CreateSFXInstance("si_Gem_Rare_Dawn Explode", transform.position);
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, dawn.explosionRadius, dawn.affectedTargets);
        for(int i = 0; i < targets.Count; i++)
        {
            info.owner.DoMagicDamage(targets[i], dawn.damageAmount[dawn.level], false, this);
        }
        photonView.RPC("RpcExplosionEffect", RpcTarget.All);
        Despawn();
    }
    
    [PunRPC]
    private void RpcExplosionEffect()
    {
        circle.Stop();
        explosion.Play();
    }

}
