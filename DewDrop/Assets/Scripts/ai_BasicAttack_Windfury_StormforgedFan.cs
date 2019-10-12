using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_BasicAttack_Windfury_StormforgedFan : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;
    private ParticleSystem hit;
    private List<LivingThing> targets = new List<LivingThing>();
    private float timer;

    public TargetValidator tv;
    public float duration = 0.4f;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        transform.LookAt(info.target.transform.position + info.target.GetCenterOffset());
        start.Play();
    }

    protected override void AliveUpdate()
    {
        timer += Time.deltaTime;
        if (!photonView.IsMine) { return; }

        if (timer >= duration)
        {
            if (targets.Count != 0)
            {
                foreach(LivingThing target in targets)
                {
                    if (!tv.Evaluate(info.owner, target)) { return; }
                    GameObject hitEffect = Instantiate(hit.gameObject, target.transform.position + target.GetCenterOffset(), Quaternion.identity);
                    hitEffect.GetComponent<ParticleSystem>().Play();
                    hitEffect.AddComponent<ParticleSystemAutoDestroy>();
                    info.owner.DoBasicAttackImmediately(target);
                }
                DetachChildParticleSystemsAndAutoDelete();
                DestroySelf();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null || target == info.owner) { return; }

        targets.Add(target);
    }
}
