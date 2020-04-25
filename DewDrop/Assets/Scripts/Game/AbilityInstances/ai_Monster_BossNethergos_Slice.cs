using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_BossNethergos_Slice : AbilityInstance
{
    public float delay = 1f;
    public float colliderActiveTime = 0.1f;
    public float damage = 50f;
    public float airborneDistance = 3f;
    public float airborneDuration = 0.75f;
    public float slowAmount = 40f;
    public float slowTime = 1.75f;
    public TargetValidator targetValidator;


    private bool acceptCollisions = false;
    private ParticleSystem pre;
    private ParticleSystem slice;
    private new GameObject collider;
    private List<Collider> affectedColliders = new List<Collider>();

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        pre = transform.Find("Pre").GetComponent<ParticleSystem>();
        slice = transform.Find("Slice").GetComponent<ParticleSystem>();
        collider = transform.Find("Colliders").gameObject;
        pre.Play();
        collider.SetActive(false);
        StartCoroutine(CoroutineSlice());
    }

    private IEnumerator CoroutineSlice()
    {
        yield return new WaitForSeconds(delay);
        slice.Play();
        collider.SetActive(true);
        acceptCollisions = true;
        yield return new WaitForSeconds(colliderActiveTime);
        acceptCollisions = false;
        collider.SetActive(false);
        if (photonView.IsMine)
        {
            Despawn();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!acceptCollisions || !photonView.IsMine || affectedColliders.Contains(other)) return;
        affectedColliders.Add(other);
        Entity thing = other.GetComponent<Entity>();
        if (thing == null) return;
        info.owner.DoMagicDamage(thing, damage, false, this);
        thing.ApplyStatusEffect(StatusEffect.Slow(slowTime, slowAmount), this);
        thing.StartDisplacement(Displacement.ByVector((thing.transform.position - info.owner.transform.position).normalized * airborneDistance, airborneDuration, false, false, false, Ease.EaseOutSine));

    }
}
