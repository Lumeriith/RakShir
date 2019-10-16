using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Huntress_Ambush : AbilityInstance
{
    public float marginToTarget = 1f;
    public float dashSpeed = 12;
    public float sliceChannelDuration = 0.2f;
    public float damage = 70;
    public float dashAnimationDuration;
    public float sliceAnimationDuration;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        StartCoroutine(CoroutineAmbush());

    }

    IEnumerator CoroutineAmbush()
    {
        Vector3 dashPosition = info.target.transform.position + (info.owner.transform.position - info.target.transform.position).normalized * marginToTarget;
        float dashDuration = Vector3.Distance(info.owner.transform.position, dashPosition) / dashSpeed;
        info.owner.DashThroughForDuration(dashPosition, dashDuration);
        info.owner.LookAt(dashPosition, true);
        info.owner.PlayCustomAnimation("Huntress - Ambush - Dash", dashAnimationDuration);
        yield return new WaitForSeconds(dashDuration);
        info.owner.DoMagicDamage(70, info.target);
        info.owner.LookAt(info.target.transform.position, true);
        info.owner.PlayCustomAnimation("Huntress - Ambush - Slice", sliceAnimationDuration);
        info.owner.control.StartChanneling(new Channel(new SelfValidator(), sliceChannelDuration, false, false, false, false, null, null));
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
