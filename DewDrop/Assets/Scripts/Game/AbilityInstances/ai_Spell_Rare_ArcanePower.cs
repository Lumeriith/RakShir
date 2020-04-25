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
            info.owner.DoMagicDamage(targets[i], damage, false, this);
        }
        info.owner.DoManaHeal(info.owner, manaHealPerHit * targets.Count, false, this);
        
        Despawn();
    }
}
