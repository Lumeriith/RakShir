using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Vault : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Vault", owner.transform.position + owner.GetCenterOffset(), info.directionQuaternion, info, new SourceInfo());

        StartCooldown();
    }

}
