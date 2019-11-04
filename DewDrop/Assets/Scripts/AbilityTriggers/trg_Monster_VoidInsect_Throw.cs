using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_VoidInsect_Throw : AbilityTrigger
{
    private CastInfo modifiedInfo;
    public override void OnCast(CastInfo info)
    {
        modifiedInfo = info;
        modifiedInfo.directionVector = (info.target.transform.position - info.owner.transform.position).normalized;
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.85f, false, true, false, false, ChannelFinished, null));
        StartCooldown();
    }

    private void ChannelFinished()
    {
        Vector3 left = Vector3.Cross(modifiedInfo.directionVector, Vector3.up);
        Vector3 original = modifiedInfo.directionVector;
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_VoidInsect_Throw", info.owner.bottom.position, modifiedInfo.directionQuaternion, modifiedInfo);
        modifiedInfo.directionVector = (original + left*0.25f).normalized;
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_VoidInsect_Throw", info.owner.bottom.position, modifiedInfo.directionQuaternion, modifiedInfo);
        modifiedInfo.directionVector = (original - left*0.25f).normalized;
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_VoidInsect_Throw", info.owner.bottom.position, modifiedInfo.directionQuaternion, modifiedInfo);

        info.owner.control.StartChanneling(new Channel(selfValidator, 0.3f, false, true, false, false, null, null));
    }
}
