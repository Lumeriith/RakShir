using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_BookOfStrength : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        info.owner.stat.strength += 1f;
        if (photonView.IsMine)
        {
            info.owner.stat.SyncSecondaryStats();
        }
        Despawn(info.owner);
    }


}
