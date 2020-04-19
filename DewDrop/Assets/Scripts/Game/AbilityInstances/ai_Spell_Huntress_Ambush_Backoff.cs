using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Huntress_Ambush_Backoff : AbilityInstance
{
    public float backoffDistance = 3.5f;
    public float backoffDuration = 0.5f;
    public float backoffAnimationDuration = 0.5f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        info.owner.LookAt(castInfo.target.transform.position);
        info.owner.PlayCustomAnimation("Huntress - Ambush - Backoff", backoffAnimationDuration);
        info.owner.StartDisplacement(new Displacement((info.owner.transform.position - info.target.transform.position).normalized * backoffDistance, backoffDuration, true, false, EasingFunction.Ease.EaseOutCubic));
        DestroySelf();
    }
}
