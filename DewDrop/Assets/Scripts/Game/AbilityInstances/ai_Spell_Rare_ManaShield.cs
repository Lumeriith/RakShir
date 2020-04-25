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

    private SFXInstance loopSFX;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        if (photonView.IsMine)
        {

            chargeCoroutine = StartCoroutine(CoroutineChargeShield());
            SFXManager.CreateSFXInstance("si_Spell_Rare_ManaShield On", transform.position);
            loopSFX = SFXManager.CreateSFXInstance("si_Spell_Rare_ManaShield Loop", transform.position);
            loopSFX.Follow(this);
        }
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
        
    }

    private IEnumerator CoroutineChargeShield()
    {
        while (true)
        {
            if (shield == null || !shield.isAlive)
            {
                shield = StatusEffect.Shield(2f, shieldChargeAmount);
                info.owner.ApplyStatusEffect(shield, this);
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
            if (loopSFX != null) loopSFX.DestroyFadingOut(0.5f);
            SFXManager.CreateSFXInstance("si_Spell_Rare_ManaShield Off", transform.position);
            if (shield.isAlive) shield.Remove();
            StopCoroutine(chargeCoroutine);
            Despawn(info.owner, DespawnBehaviour.StopAndWaitForParticleSystems);
        }
    }
}
