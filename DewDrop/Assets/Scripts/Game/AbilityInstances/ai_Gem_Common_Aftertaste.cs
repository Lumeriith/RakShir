using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Common_Aftertaste : AbilityInstance
{
    public float delay = 1f;

    private bool isDamage;
    private float amount;

    private SFXInstance start;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        isDamage = (bool)data[0];
        amount = (float)data[1];
        if (isMine) StartCoroutine("CoroutineAftertaste");
    }

    protected override void AliveUpdate()
    {
        transform.position = info.target.transform.position + info.target.GetCenterOffset();
    }

    private IEnumerator CoroutineAftertaste()
    {
        start = SFXManager.CreateSFXInstance("si_Gem_Common_Aftertaste Start", transform.position);
        start.Follow(this);
        yield return new WaitForSeconds(delay);
        start.Stop();
        SFXManager.CreateSFXInstance("si_Gem_Common_Aftertaste Hit", transform.position);
        if (isDamage)
        {
            info.owner.DoMagicDamage(amount * ((gem_Common_Aftertaste)source.gem).bonusMagicDamagePercentage[source.gem.level] / 100f, info.target, false, source);
        }
        else
        {
            info.owner.DoHeal(amount * ((gem_Common_Aftertaste)source.gem).bonusHealPercentage[source.gem.level] / 100f, info.target, false, source);
        }
        DetachChildParticleSystemsAndAutoDelete(DespawnBehaviour.WaitForParticleSystems, info.target);
        Despawn();
    }
}
