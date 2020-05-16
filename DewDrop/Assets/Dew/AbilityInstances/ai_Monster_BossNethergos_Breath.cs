using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_BossNethergos_Breath : AbilityInstance
{
    public float delay = 1.75f;
    private Transform follow;
    private new GameObject collider;
    private ParticleSystem breath;
    private bool acceptCollisions = false;
    public float tickTime = 0.25f;
    public int tickCount = 10;
    public float damage = 30f;
    public float silenceDuration = 0.4f;
    private List<Collider> affectedColliders = new List<Collider>();
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        follow = info.owner.transform.FindDeepChild("Breath Pivot");
        transform.position = follow.position;
        transform.rotation = follow.rotation;
        collider = transform.Find("Collider").gameObject;
        collider.SetActive(false);
        breath = transform.Find("Breath").GetComponent<ParticleSystem>();
        StartCoroutine(CoroutineBreath());
    }

    protected override void AliveUpdate()
    {
        transform.position = follow.position;
        transform.rotation = follow.rotation;
    }


    private IEnumerator CoroutineBreath()
    {
        yield return new WaitForSeconds(delay);
        breath.Play();
        collider.SetActive(true);
        acceptCollisions = true;
        for(int i = 0; i < tickCount; i++)
        {
            affectedColliders.Clear();
            yield return new WaitForSeconds(tickTime);
        }
        acceptCollisions = false;
        collider.SetActive(false);
        if (photonView.IsMine)
        {
            Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
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
        thing.ApplyStatusEffect(StatusEffect.Silence(silenceDuration), this);
    }
}
