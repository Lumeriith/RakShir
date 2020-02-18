using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_ArcanePower : AbilityInstance
{
    public TargetValidator targetValidator;
    public float radius = 4f;
    public float damage = 80f;
    public float manaHealPerHit = 20f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (!photonView.IsMine) return;
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
        for(int i = 0; i < targets.Count; i++)
        {
            info.owner.DoMagicDamage(damage, targets[i], false, source);
        }
        info.owner.DoManaHeal(manaHealPerHit * targets.Count, info.owner, false, source);
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }
}
