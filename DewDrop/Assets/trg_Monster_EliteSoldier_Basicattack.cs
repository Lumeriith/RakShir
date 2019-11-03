using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_EliteSoldier_Basicattack : AbilityTrigger
{


    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Monster_EliteSoldier_Basicattack", transform.position, Quaternion.identity);
        StartCooldown(true);
    }
}
