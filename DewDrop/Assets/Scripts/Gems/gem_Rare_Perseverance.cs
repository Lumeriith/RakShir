using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Perseverance : Gem
{
    public float[] shieldDuration = { 3.0f, 3.5f, 4.0f, 4.5f };
    public float[] shieldAmount = { 45, 65, 85, 105 };
    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnTriggerCast(bool isMine)
    {
        if (isMine) CreateAbilityInstance("ai_Gem_Rare_Perseverance", owner.transform.position + owner.GetCenterOffset(), Quaternion.identity);
    }
}
