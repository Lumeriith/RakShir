using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Swiftness : Gem
{
    public float[] speedDuration = { 1.5f, 1.75f, 2.0f, 2.25f };
    public float[] speedAmount = { 20.0f, 27.5f, 35f, 42.5f };
    public override void OnEquip(LivingThing owner, AbilityTrigger trigger)
    {
        
    }

    public override void OnUnequip(LivingThing owner, AbilityTrigger trigger)
    {

    }

    public override void OnTriggerCast(bool isMine)
    {
        if (isMine) owner.ApplyStatusEffect(StatusEffect.Speed(source, speedDuration[level], speedAmount[level]));
    }
}
