using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_cons_GoldPouch : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if(photonView.IsMine) info.owner.EarnGold((float)data[0]);
        Despawn(info.owner);
    }



}
