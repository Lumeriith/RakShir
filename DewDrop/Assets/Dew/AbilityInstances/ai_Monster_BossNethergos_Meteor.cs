using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Monster_BossNethergos_Meteor : AbilityInstance
{
    public float damage = 90f;
    public float slowDuration = 0.5f;
    public float slowAmount = 80f;
    public float radius;
    public TargetValidator targetValidator;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        StartCoroutine("CoroutineExplode");

    }

    IEnumerator CoroutineExplode()
    {
        yield return new WaitForSeconds(1.666f);
        if (photonView.IsMine)
        {
            List<Entity> targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].statusEffect.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), this);
                info.owner.DoMagicDamage(targets[i], damage, false, this);
            }
        }
        
        Despawn();
    }
}
