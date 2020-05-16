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
        info.owner.StartDisplacement(Displacement.TowardsTarget(info.target, marginToTarget, dashSpeed, true, true, Slice, Canceled));
        info.owner.PlayCustomAnimation("Huntress - Ambush - Dash", dashAnimationDuration);
    }

    private void Slice()
    {
        info.owner.DoMagicDamage(info.target, 70, false, this);
        info.owner.LookAt(info.target.transform.position, true);
        info.owner.PlayCustomAnimation("Huntress - Ambush - Slice", sliceAnimationDuration);
        info.owner.control.StartChanneling(new Channel(new SelfValidator(), sliceChannelDuration, false, false, false, false, null, null));
        Despawn();
    }

    private void Canceled()
    {
        Despawn();
    }



}
