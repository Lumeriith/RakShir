using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Epic_Sympathy : Gem
{
    public float[] splashPercentage = { 80, 120, 160 };
    public float splashWidth = 1f;
    public TargetValidator affectedTargets;

    public override void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstance instance)
    {
        if (isMine) instance.OnDealMagicDamage += DealtMagicDamage;
    }

    private void DealtMagicDamage(InfoMagicDamage info)
    {
        CreateAbilityInstance("ai_Gem_Epic_Sympathy", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, CastInfo.OwnerAndTarget(info.from, info.to), new object[] { info.finalDamage });
    }
}
