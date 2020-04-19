using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_CurrentOfSwiftness : AbilityInstance
{
    private StatusEffect shield;
    private StatusEffect speed;
    private StatusEffect haste;
    public float shieldAmount = 70f;
    public float shieldDuration = 12f;
    public float speedAmount = 20f;
    public float hasteAmount = 25f;
    private SFXInstance sfx;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        if (!photonView.IsMine) return;
        shield = StatusEffect.Shield(source, shieldDuration, shieldAmount);
        speed = StatusEffect.Speed(source, shieldDuration, speedAmount);
        haste = StatusEffect.Haste(source, shieldDuration, hasteAmount);
        info.owner.statusEffect.ApplyStatusEffect(shield);
        info.owner.statusEffect.ApplyStatusEffect(speed);
        info.owner.statusEffect.ApplyStatusEffect(haste);
        sfx = SFXManager.CreateSFXInstance("si_Spell_Rare_CurrentOfSwiftness", transform.position);
        sfx.Follow(this);
    }


    protected override void AliveUpdate()
    {
        transform.rotation = Quaternion.identity;
        if (!photonView.IsMine) return;
        if (shield == null || !shield.isAlive || speed == null || !speed.isAlive || haste == null || !haste.isAlive)
        {
            sfx.DestroyFadingOut(1.5f);
            SFXManager.CreateSFXInstance("si_Spell_Rare_CurrentOfSwiftness Off", transform.position);
            if (speed != null && speed.isAlive) speed.Remove();
            if (haste != null && haste.isAlive) haste.Remove();
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }
    }
}
