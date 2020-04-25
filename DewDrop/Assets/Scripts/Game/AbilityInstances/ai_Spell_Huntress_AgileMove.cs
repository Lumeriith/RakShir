using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Huntress_AgileMove : AbilityInstance
{
    public float speedAmount = 20f;
    public float dodgeAmount = 40f;
    public float duration = 4f;
    public float shieldAmount = 50f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        StartCoroutine("CoroutineAgileMove");
        transform.parent = info.owner.transform;
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
    }

    IEnumerator CoroutineAgileMove()
    {
        if (info.owner.photonView.IsMine)
        {
            info.owner.statusEffect.ApplyStatusEffect(StatusEffect.Speed(duration, speedAmount), reference);
            info.owner.statusEffect.ApplyStatusEffect(StatusEffect.Shield(duration, shieldAmount), reference);
        }
        info.owner.stat.bonusDodgeChance += dodgeAmount;
        yield return new WaitForSeconds(duration);
        info.owner.stat.bonusDodgeChance -= dodgeAmount;
        Despawn(info.owner, DespawnBehaviour.StopAndWaitForParticleSystems);
    }
}
