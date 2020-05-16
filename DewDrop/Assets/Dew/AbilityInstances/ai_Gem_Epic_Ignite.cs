using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Gem_Epic_Ignite : AbilityInstance
{
    private gem_Epic_Ignite ignite;

    private ParticleSystem ready;
    private ParticleSystem distanceEmitter;
    private ParticleSystem hit;

    private bool didHit = false;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        ignite = (gem_Epic_Ignite)gem;
        ready = transform.Find<ParticleSystem>("Ready");
        distanceEmitter = transform.Find<ParticleSystem>("Distance Emitter");
        hit = transform.Find<ParticleSystem>("Hit");
        ready.Play();
        if (!isMine) return;
        info.owner.OnDoBasicAttackHit += DidBasicAttackHit;
        SFXManager.CreateSFXInstance("si_Gem_Epic_Ignite Ready", transform.position);
    }

    protected override void AliveUpdate()
    {
        if (!didHit)
        {
            transform.position = info.owner.transform.position;
        }
    }

    private void DidBasicAttackHit(InfoBasicAttackHit info)
    {
        // TODO fix?
        //if (info.reference.instance.creationTime < creationTime) return;
        this.info.owner.OnDoBasicAttackHit -= DidBasicAttackHit;
        photonView.RPC("RpcHit", RpcTarget.All, info.to.photonView.ViewID);
        info.to.ApplyStatusEffect(StatusEffect.DamageOverTime(ignite.damageDuration, ignite.damageAmount[ignite.level]), this);
        SFXManager.CreateSFXInstance("si_Gem_Epic_Ignite Hit", transform.position);
    }

    [PunRPC]
    private void RpcHit(int id)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(id);
        if (view == null) return;
        Entity thing = view.GetComponent<Entity>();
        if (thing == null) return;
        distanceEmitter.transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        distanceEmitter.Play();

        hit.transform.position = thing.transform.position + thing.GetCenterOffset();
        hit.transform.parent = thing.transform;
        hit.gameObject.AddComponent<ParticleSystemAutoDestroy>();
        hit.Play();
        ready.Stop();
        StartCoroutine(CoroutineMoveDistanceEmitter(thing));
        didHit = true;
    }

    private IEnumerator CoroutineMoveDistanceEmitter(Entity thing)
    {
        yield return null;
        Debug.Log("Moved" + thing.name);
        distanceEmitter.transform.position = thing.transform.position + thing.GetCenterOffset();
        yield return null;
        distanceEmitter.Stop();
        
        if (isMine)
        {
            
            Despawn();
        }
    }
}
