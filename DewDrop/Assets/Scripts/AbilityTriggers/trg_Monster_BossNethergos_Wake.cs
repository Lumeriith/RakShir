using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_BossNethergos_Wake : AbilityTrigger
{
    public float wakeupDuration = 8f;
    public override void OnCast(CastInfo info)
    {
        info.owner.ApplyStatusEffect(StatusEffect.Stasis(info.owner, wakeupDuration));
        info.owner.ApplyStatusEffect(StatusEffect.Unstoppable(info.owner, 3600f * 24f));
        StartCooldown();
    }
}
