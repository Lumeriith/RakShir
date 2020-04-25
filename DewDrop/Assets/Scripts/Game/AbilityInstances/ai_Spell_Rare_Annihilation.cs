using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_Annihilation : AbilityInstance
{
    public float tickInterval = 0.5f;
    public float damage = 15f;
    public float radius = 4.5f;
    
    public TargetValidator affectedTargets;

    private ParticleSystem aura;

    private SFXInstance loop;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        aura = transform.Find<ParticleSystem>("Aura");
        aura.Play();
        if(isMine) StartCoroutine("CoroutineAnnihilation");
    }

    private IEnumerator CoroutineAnnihilation()
    {
        loop = SFXManager.CreateSFXInstance("si_Spell_Rare_Annihilation Loop", transform.position);
        loop.Follow(this);
        
        List<LivingThing> targets;
        while (true)
        {
            targets = info.owner.GetAllTargetsInRange(transform.position, radius, affectedTargets);
            for(int i = 0; i < targets.Count; i++)
            {
                info.owner.DoMagicDamage(targets[i], damage, false, this);
                SFXManager.CreateSFXInstance("si_Spell_Rare_Annihilation Hit", targets[i].transform.position);
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
        transform.rotation = Quaternion.identity;
    }

    protected override void OnReceiveEvent(string eventString)
    {
        if(eventString == "EndAura")
        {
            loop.DestroyFadingOut(1f);
            Despawn(info.owner, DespawnBehaviour.StopAndWaitForParticleSystems);
            Despawn();
        }
    }

}
