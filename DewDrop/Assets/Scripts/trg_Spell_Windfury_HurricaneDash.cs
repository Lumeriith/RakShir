using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Windfury_HurricaneDash : AbilityTrigger
{
    private CastInfo info;
    private float distance = 0;
    private object[] data = new object[1];

    public float bonusDistancePerMovementSpeed = 0.02f;

    public override void OnCast(CastInfo info)
    {
        this.info = info;
        distance = info.owner.stat.finalMovementSpeed * bonusDistancePerMovementSpeed;
        data[0] = distance;
        indicator.arrowLength = distance;
        Channel channel = new Channel(selfValidator, 0, false, false, false, false, Success, null);
        info.owner.control.StartChanneling(channel);
        StartCooldown();
    }

    private void Success()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Windfury_HurricaneDash", info.owner.transform.position + info.owner.GetCenterOffset(), info.directionQuaternion, info, data);
    }
}
