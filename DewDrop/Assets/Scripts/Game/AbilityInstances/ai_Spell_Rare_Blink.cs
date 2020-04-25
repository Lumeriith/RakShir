using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Blink : AbilityInstance
{
    private ParticleSystem start;
    private ParticleSystem end;

    public TargetValidator targetValidator;
    public float damage = 80f;
    public float cooldownReduction = 6f;
    public float radius = 1.5f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        start = transform.Find("Start").GetComponent<ParticleSystem>();
        end = transform.Find("End").GetComponent<ParticleSystem>();
        start.transform.position = info.owner.transform.position;
        end.transform.position = info.point;
        start.Play();
        end.Play();
        if (photonView.IsMine)
        {
            info.owner.Teleport(info.point);
            List<LivingThing> targets = info.owner.GetAllTargetsInRange(info.owner.transform.position, radius, targetValidator);
            if (targets.Count > 0) info.owner.control.skillSet[3].ApplyCooldownReduction(cooldownReduction);
            for(int i = 0; i < targets.Count; i++)
            {
                info.owner.DoMagicDamage(targets[i], damage, false, reference);
            }
            
            Despawn();
        }
    }
}
