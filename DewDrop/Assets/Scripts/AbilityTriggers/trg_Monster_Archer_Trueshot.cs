using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_Archer_Trueshot : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 1.2f, false, false, false, false, ChannelSuccess, null));
        StartCooldown();
        StartCoroutine(CoroutineLook());
    }

    IEnumerator CoroutineLook()
    {
        float now = Time.time;
        while(Time.time - now < 1.1f)
        {
            info.owner.LookAt(info.target.transform.position, true);
            yield return null;
        }
        
    }

    private void ChannelSuccess()
    {
        AbilityInstanceManager.CreateAbilityInstance("ai_Monster_Archer_Trueshot", info.owner.transform.position + info.owner.GetCenterOffset(), info.owner.transform.rotation, info);
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.5f, false, false, false, false, null, null));
    }
}
