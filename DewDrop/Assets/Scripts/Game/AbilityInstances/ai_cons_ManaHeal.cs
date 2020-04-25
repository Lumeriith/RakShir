using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_ManaHeal : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;

        info.owner.DoManaHeal(info.owner, (float)data[0], true, this);
        Despawn(info.owner);
    }
}
