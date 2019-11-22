using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_BossNethergos_Slice : AbilityTrigger
{
    public float channelDuration = 3f;
    public override void OnCast(CastInfo info)
    {
        Quaternion rotation = Quaternion.LookRotation(info.target.transform.position - info.owner.transform.position, Vector3.up);
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, null, null));
        CreateAbilityInstance("ai_Monster_BossNethergos_Slice", info.owner.transform.position, rotation);
        StartCooldown();
    }
}
