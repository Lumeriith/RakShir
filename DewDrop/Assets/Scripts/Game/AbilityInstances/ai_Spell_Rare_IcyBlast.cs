using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_IcyBlast : AbilityInstance
{
    public TargetValidator targetValidator;
    public float radius = 4f;

    public float slowDuration = 3f;
    public float slowAmount = 40f;
    public float rootDuration = 1f;
    public float damage = 60f;
    public float secondBlastDelay = 1.5f;

    private ParticleSystem range;
    private ParticleSystem blast;
    private GameObject hit;
    private GameObject root;


    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        range = transform.Find("Range").GetComponent<ParticleSystem>();
        blast = transform.Find("Blast").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").gameObject;
        root = transform.Find("Root").gameObject;

        range.Play();
        if (photonView.IsMine)
        {
            StartCoroutine(CoroutineIcyBlast());
        }
    }

    private IEnumerator CoroutineIcyBlast()
    {
        Explode();
        yield return new WaitForSeconds(secondBlastDelay);
        Explode();
        Despawn();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }

    private void Explode()
    {
        SFXManager.CreateSFXInstance("si_Spell_Rare_IcyBlast Blast", transform.position);
        photonView.RPC("RpcBlast", RpcTarget.All);
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].statusEffect.IsAffectedBy(StatusEffectType.Slow))
            {
                targets[i].statusEffect.ApplyStatusEffect(StatusEffect.Root(rootDuration), this);
                photonView.RPC("RpcRoot", RpcTarget.All, targets[i].photonView.ViewID);
            }
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
            targets[i].statusEffect.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), this);
            SFXManager.CreateSFXInstance("si_Spell_Rare_IcyBlast Hit", transform.position);
            info.owner.DoMagicDamage(targets[i], damage, false, this);
        }
    }

    [PunRPC]
    private void RpcBlast()
    {
        blast.Play();
    }

    [PunRPC]
    private void RpcRoot(int viewID)
    {
        Transform target = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(root, target.position, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        Transform target = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(hit, target.position, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }


    private void ChannelCanceled()
    {
        Despawn(DespawnBehaviour.Immediately);
    }
}
