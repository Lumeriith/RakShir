using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_EliteSoldier_Basicattack : AbilityInstance
{
    public float range = 4f;
    public float width = 1f;

    public SelfValidator selfValidator;

    // Hit: 0.65
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {
            
        }
        
    }
}
