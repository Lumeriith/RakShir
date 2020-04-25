using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Common_Preservation : Gem
{
    public float[] shieldPercentage = { 20, 35, 50, 65, 80 };
    public float shieldDuration = 4f;

    public override void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstanceSafeReference reference)
    {
        reference.OnDoHeal += DidHeal;
    }

    private void DidHeal(InfoHeal info)
    {
        info.to.ApplyStatusEffect(StatusEffect.Shield(shieldDuration, shieldPercentage[level]), null);
        // TODO better
    }
}
