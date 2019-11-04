using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_ManaShield : AbilityInstance
{
    public StatusEffect shield; // Owner only!
    private Coroutine chargeCoroutine;
    public float shieldMaxAmount = 300f;
    public float shieldChargeAmount = 20f;
    public float shieldChargeInterval = 0.5f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if(photonView.IsMine) chargeCoroutine = StartCoroutine(CoroutineChargeShield());
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
    }

    private IEnumerator CoroutineChargeShield()
    {
        while (true)
        {
            if (shield == null || !shield.isAlive)
            {
                shield = StatusEffect.Shield(info.owner, 2f, shieldChargeAmount);
                info.owner.ApplyStatusEffect(shield);
            }
            else
            {
                shield.SetDuration(2f);
                shield.SetParameter(Mathf.Clamp((float)shield.parameter + shieldChargeAmount * (info.owner.stat.finalSpellPower / 100f), 0, shieldMaxAmount));
            }
            yield return new WaitForSeconds(shieldChargeInterval);
        }
    }

    protected override void AliveUpdate()
    {

    }

    protected override void OnReceiveEvent(string eventString)
    {
        if (!photonView.IsMine) return;
        if(eventString == "Off")
        {
            if(shield.isAlive) shield.Remove();
            StopCoroutine(chargeCoroutine);
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
