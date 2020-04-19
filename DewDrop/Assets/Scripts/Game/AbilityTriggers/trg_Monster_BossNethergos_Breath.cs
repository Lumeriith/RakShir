using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_BossNethergos_Breath : AbilityTrigger
{
    public float channelDuration = 7f;
    public override void OnCast(CastInfo info)
    {
        Quaternion rotation = Quaternion.LookRotation(info.target.transform.position - info.owner.transform.position, Vector3.up);
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, null, null));
        CreateAbilityInstance("ai_Monster_BossNethergos_Breath", info.owner.transform.position, rotation);
        StartCooldown();
    }
}
