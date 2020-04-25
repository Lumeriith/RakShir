using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Rare_Perseverance : AbilityInstance
{
    private gem_Rare_Perseverance perseverance;
    private StatusEffect shield;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        transform.parent = info.owner.transform;
        if (!isMine) return;
        perseverance = (gem_Rare_Perseverance)gem;
        shield = StatusEffect.Shield(perseverance.shieldDuration[perseverance.level], perseverance.shieldAmount[perseverance.level]);
        info.owner.ApplyStatusEffect(shield, this);
        shield.OnExpire += ShieldExpired;
    }

    private void ShieldExpired()
    {
        Despawn(info.owner, AttachBehaviour.IgnoreRotation);
    }

}
