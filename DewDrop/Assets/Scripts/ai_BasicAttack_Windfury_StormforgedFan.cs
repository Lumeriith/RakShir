using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_BasicAttack_Windfury_StormforgedFan : AbilityInstance
{
    private CastInfo info;
    private ParticleSystem start;
    private ParticleSystem hit;
    private ParticleSystem land;
    private List<LivingThing> targets = new List<LivingThing>();
    private Vector3 targetPosition = Vector3.zero;

    public float scaleMagnificationValue = 3f;
    public float distance = 3f;
    public float ejectionForce = 10f;

    private void Awake()
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        this.info = castInfo;
        transform.Rotate(info.directionVector.x, transform.rotation.y, info.directionVector.z);
        targetPosition = transform.position + (info.target.transform.position + info.target.GetCenterOffset() - transform.position).normalized * distance;

        start.Play();
    }

    protected override void AliveUpdate()
    {
        float scaleDeltaX = transform.localScale.x * (1 + scaleMagnificationValue * Time.deltaTime);
        transform.localScale = new Vector3(scaleDeltaX, transform.localScale.y, transform.localScale.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, ejectionForce * Time.deltaTime);

        if (!photonView.IsMine) { return; }

        if (Vector3.Distance(transform.position, targetPosition) <= float.Epsilon)
        {
            if (targets.Count != 0)
            {
                foreach(LivingThing target in targets)
                {
                    GameObject hitEffect = Instantiate(hit.gameObject, target.transform.position + target.GetCenterOffset(), Quaternion.identity);
                    hitEffect.GetComponent<ParticleSystem>().Play();
                    hitEffect.AddComponent<ParticleSystemAutoDestroy>();
                    info.owner.DoBasicAttackImmediately(target);
                }
                land.Play();
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
