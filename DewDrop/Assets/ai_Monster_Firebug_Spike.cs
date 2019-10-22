using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Firebug_Spike : AbilityInstance
{
    public float distance;
    public float speed;
    public float damage;
    public float airborneDuration;
    public float airborneDistance;

    public TargetValidator targetValidator;

    private Vector3 startPosition;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.rotation = castInfo.directionQuaternion;
        startPosition = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (!photonView.IsMine) return;

        if(Vector3.Distance(transform.position, startPosition) > distance)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null) return;
        if (!targetValidator.Evaluate(info.owner, lv)) return;
        info.owner.DoMagicDamage(damage, lv);
        Vector3 landPos = lv.transform.position + (lv.transform.position - transform.position).normalized * airborneDistance;
        lv.AirborneForDuration(landPos, airborneDuration);
    }
}
