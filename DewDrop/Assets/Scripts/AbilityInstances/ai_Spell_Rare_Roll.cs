using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Roll : AbilityInstance
{
    public float distance = 3f;
    public float duration = 0.7f;

    public float attackDamageBoostAmount = 50f;
    public float attackDamageBoostDuration = 4f;

    private StatusEffect damageBoost;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {
            info.owner.StartDisplacement(new Displacement(info.directionVector * distance, duration, true, true));
            info.owner.LookAt(transform.position + info.directionVector, true);
            StartCoroutine("CoroutineApplyDamageBoost");
        }
    }

    private IEnumerator CoroutineApplyDamageBoost()
    {
        yield return new WaitForSeconds(duration);
        damageBoost = StatusEffect.AttackDamageBoost(info.owner, attackDamageBoostDuration, attackDamageBoostAmount);
        info.owner.ApplyStatusEffect(damageBoost);
        damageBoost.OnExpire += EffectExpired;
        info.owner.OnDoBasicAttackHit += EffectUsed;
    }

    private void EffectExpired()
    {
        info.owner.OnDoBasicAttackHit -= EffectUsed;
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    private void EffectUsed(InfoBasicAttackHit info)
    {
        damageBoost.OnExpire -= EffectExpired;
        this.info.owner.OnDoBasicAttackHit -= EffectUsed;
        damageBoost.Remove();
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }


}
