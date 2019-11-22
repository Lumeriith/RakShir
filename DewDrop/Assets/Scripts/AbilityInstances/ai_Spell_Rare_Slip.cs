using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Slip : AbilityInstance
{
    public float duration = 1f;
    public float distance = 4.5f;

    public float damage = 60f;
    public TargetValidator validator;
    private float elapsedTime = 0f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.rotation = castInfo.directionQuaternion;
        Vector3 posA = transform.position + info.directionVector * distance * 3f / 4f;
        Vector3 posB = transform.position + info.directionVector * distance;

        


        if (photonView.IsMine)
        {
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true, EasingFunction.Ease.EaseOutQuad, StopSlip, StopSlip));
        }
    }



    private void StopSlip()
    {
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
        elapsedTime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (elapsedTime > duration) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null) return;
        if (!validator.Evaluate(info.owner, thing)) return;
        thing.ApplyStatusEffect(StatusEffect.Stun(info.owner, duration - elapsedTime));
        info.owner.DoMagicDamage(damage, thing);
    }
}
