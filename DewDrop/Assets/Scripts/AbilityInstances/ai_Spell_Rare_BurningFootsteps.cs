using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_BurningFootsteps : AbilityInstance
{
    public int ticks = 12;
    public float tickInterval = 0.5f;
    public float damage = 15f;
    public float radius = 1f;
    public TargetValidator targetValidator;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine) StartCoroutine(CoroutineDamage());
    }

    private IEnumerator CoroutineDamage()
    {
        List<LivingThing> targets;
        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
            targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                info.owner.DoMagicDamage(damage, targets[j]);
            }
        }
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }
}
