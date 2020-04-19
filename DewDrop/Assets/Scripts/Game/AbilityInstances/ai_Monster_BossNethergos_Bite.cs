using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_BossNethergos_Bite : AbilityInstance
{
    public float delay = 1.9f;
    public float colliderActiveTime = 0.1f;
    public float damage = 100f;
    public TargetValidator targetValidator;


    private bool acceptCollisions = false;
    private ParticleSystem pre;
    private ParticleSystem bite;
    private new GameObject collider;
    private List<Collider> affectedColliders = new List<Collider>();

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        pre = transform.Find("Pre").GetComponent<ParticleSystem>();
        bite = transform.Find("Bite").GetComponent<ParticleSystem>();
        collider = transform.Find("Collider").gameObject;
        pre.Play();
        collider.SetActive(false);
        StartCoroutine(CoroutineBite());
    }

    private IEnumerator CoroutineBite()
    {
        yield return new WaitForSeconds(delay);
        bite.Play();
        collider.SetActive(true);
        acceptCollisions = true;
        yield return new WaitForSeconds(colliderActiveTime);
        acceptCollisions = false;
        collider.SetActive(false);
        if (photonView.IsMine)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!acceptCollisions || !photonView.IsMine || affectedColliders.Contains(other)) return;
        affectedColliders.Add(other);
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null) return;
        info.owner.DoMagicDamage(damage, thing, false, source);
    }
}
