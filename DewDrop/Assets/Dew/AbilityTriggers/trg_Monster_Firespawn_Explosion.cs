using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Firespawn_Explosion : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Monster_Firespawn_Explosion", info.target.transform.position, Quaternion.identity, info);
        info.owner.control.StartChanneling(new Channel(selfValidator, 1.7f, false, false, false, false, null, null));
        StartCooldown();

    }
}
