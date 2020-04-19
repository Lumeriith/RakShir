using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Epic_Time : Gem
{
    public float[] cooldownReduction = { 0.5f, 1.0f, 1.5f };
    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.isMine) owner.OnDoBasicAttackHit += DidBasicAttackHit;
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {
        if (owner.isMine) owner.OnDoBasicAttackHit -= DidBasicAttackHit;
    }

    private void DidBasicAttackHit(InfoBasicAttackHit info)
    {
        if(trigger != null && !trigger.isCooledDown) CreateAbilityInstance("ai_Gem_Epic_Time", owner.transform.position, Quaternion.identity);
    }


}
