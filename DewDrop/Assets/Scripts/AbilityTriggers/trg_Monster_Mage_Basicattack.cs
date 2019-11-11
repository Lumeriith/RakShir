using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Mage_Basicattack : AbilityTrigger
{
    private Quaternion shootDir;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, ChannelSuccess, null, true));
        shootDir = Quaternion.LookRotation(info.target.transform.position - info.owner.transform.position, Vector3.up);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_Mage_Basicattack", info.owner.transform.position + info.owner.GetCenterOffset(), shootDir, info);
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.45f, false, false, false, false, null, null, true));
    }
}
