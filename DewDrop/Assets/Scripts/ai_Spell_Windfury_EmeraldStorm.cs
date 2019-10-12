using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Windfury_EmeraldStorm : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;
    private ParticleSystem hit;
    private List<LivingThing> targets = new List<LivingThing>();
    private List<LivingThing> eyeTargets = new List<LivingThing>();
    private float timer;
    private bool isSpawned = false;

    public float duration = 5f;
    public float damageWhenCreate = 200f;
    public float damagePerTick = 20f;
    public float tickInterval = 0.5f;
    public float eyeRadius = 3f;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        start.Play();
    }

    protected override void AliveUpdate()
    {
        if (!photonView.IsMine) { return; }
        timer += Time.deltaTime;
        transform.position = info.owner.transform.position;
        
        if (timer <= duration)
        {
            if (!isSpawned)
            {
                foreach (LivingThing target in targets)
                {
                    info.owner.DoMagicDamage(damageWhenCreate, target);
                    GameObject hitEffect = Instantiate(hit.gameObject, target.transform.position + target.GetCenterOffset(), Quaternion.identity);
                    hitEffect.GetComponent<ParticleSystem>().Play();
                    hitEffect.AddComponent<ParticleSystemAutoDestroy>();
                }
                StartCoroutine("DoTickDamage");
                isSpawned = true;
            }

            foreach (LivingThing target in eyeTargets)
            {
                if (Vector3.Distance(transform.position, target.transform.position) < eyeRadius) { continue; }
                if (!targets.Contains(target)) { targets.Add(target); }
            }
        }
        else
        {
            StopCoroutine("DoTickDamage");
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null || target == info.owner) { return; }
        if (Vector3.Distance(transform.position, target.transform.position) < eyeRadius)
        {
            if (!eyeTargets.Contains(target)) { eyeTargets.Add(target); }
            return;
        }
        targets.Add(target);
    }

    private void OnTriggerExit(Collider other)
    {
        int idx = targets.FindIndex(item => item == other.GetComponent<LivingThing>());
        if (idx == -1) { return; }
        targets.RemoveAt(idx);
    }

    private IEnumerator DoTickDamage()
    {
        while (true)
        {
            foreach (LivingThing target in targets)
            {
                if (Vector3.Distance(transform.position, target.transform.position) < eyeRadius) { continue; }

                info.owner.DoMagicDamage(damagePerTick, target);
                GameObject hitEffect = Instantiate(hit.gameObject, target.transform.position + target.GetCenterOffset(), Quaternion.identity);
                hitEffect.GetComponent<ParticleSystem>().Play();
                hitEffect.AddComponent<ParticleSystemAutoDestroy>();

            }
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
