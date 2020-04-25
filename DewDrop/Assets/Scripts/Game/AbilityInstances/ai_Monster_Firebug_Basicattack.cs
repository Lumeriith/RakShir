using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_Firebug_Basicattack : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.Find("Flash").position = castInfo.target.transform.position + castInfo.target.GetCenterOffset();
        if (!photonView.IsMine) return;
        castInfo.owner.DoBasicAttackImmediately(castInfo.target, this);
        
        Despawn();
    }

    
}
