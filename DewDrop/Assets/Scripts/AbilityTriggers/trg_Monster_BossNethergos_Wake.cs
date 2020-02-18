using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_BossNethergos_Wake : AbilityTrigger
{
    public float wakeupDuration = 8f;
    public override void OnCast(CastInfo info)
    {
        info.owner.ApplyStatusEffect(StatusEffect.Stasis(source, wakeupDuration));
        info.owner.ApplyStatusEffect(StatusEffect.Unstoppable(source, 3600f * 24f));
        PlayerViewCamera.instance.visionMultiplier *= 1.2f;
        StartCooldown();
    }
}
