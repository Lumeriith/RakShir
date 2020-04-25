using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Monster_BossNethergos_Thump : AbilityInstance
{
    public TargetValidator targetValidator;
    
    public float delay = 1f;
    public float damage = 120f;
    public float stunDuration = 3f;
    public float silenceDuration = 6f;
    public float slowDuration = 6f;
    public float slowAmount = 30f;
    public float colliderDuration = 0.65f;

    private ParticleSystem pre;
    private GameObject hit;
    private ParticleSystem stomp;
    private GameObject colliders;

    private List<Collider> affectedColliders = new List<Collider>();

    private bool acceptCollisions = false;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        pre = transform.Find("Pre").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").gameObject;
        stomp = transform.Find("Stomp").GetComponent<ParticleSystem>();
        colliders = transform.Find("Colliders").gameObject;
        colliders.SetActive(false);
        StartCoroutine(CoroutineThump());
    }

    private IEnumerator CoroutineThump()
    {
        pre.Play();
        yield return new WaitForSeconds(delay);
        acceptCollisions = true;
        pre.Stop();
        stomp.Play();
        colliders.SetActive(true);
        yield return new WaitForSeconds(colliderDuration);
        colliders.SetActive(false);
        acceptCollisions = false;
        if (photonView.IsMine)
        {
            
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Collide(other);
    }


    private void Collide(Collider other)
    {
        if (!photonView.IsMine) return;
        if (affectedColliders.Contains(other)) return;
        affectedColliders.Add(other);
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        thing.ApplyStatusEffect(StatusEffect.Stun(stunDuration), reference);
        thing.ApplyStatusEffect(StatusEffect.Silence(silenceDuration), reference);
        thing.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), reference);
        info.owner.DoMagicDamage(thing, damage, false, reference);
        photonView.RPC("RpcHit", RpcTarget.All, thing.photonView.ViewID);
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }



}
