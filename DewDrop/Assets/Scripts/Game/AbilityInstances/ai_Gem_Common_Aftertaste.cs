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
            info.owner.DoMagicDamage(info.target, amount * ((gem_Common_Aftertaste)gem).bonusMagicDamagePercentage[gem.level] / 100f, false, this);
        }
        else
        {
            info.owner.DoHeal(info.target, amount * ((gem_Common_Aftertaste)gem).bonusHealPercentage[gem.level] / 100f, false, this);
        }
        Despawn(info.target);
    }
}
