using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Consume : Gem
{
    public float[] healPercentage = { 25, 35, 45, 55 };

    public override void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstanceSafeReference reference)
    {
        reference.OnDealMagicDamage += DealtMagicDamage;
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        CreateAbilityInstance("ai_Gem_Rare_Consume", owner.transform.position, Quaternion.identity, new object[] { info.finalDamage *  healPercentage[level] / 100f });
    }
}
