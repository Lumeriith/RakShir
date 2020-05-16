using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Pursuit : AbilityTrigger
{
    public float speedAmount = 25f;
    public float speedDuration = 2f;

    private StatusEffect speed = null;

    public override void OnCast(CastInfo info)
    {

    }

    private void Update()
    {
        if(speed == null || !speed.isAlive)
        {
            SetSpecialFillAmount(0f);
        }
        else
        {
            SetSpecialFillAmount(speed.duration / speedDuration);
        }
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit += BasicAttackHit;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit -= BasicAttackHit;
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        if(speed == null || !speed.isAlive)
        {
            speed = StatusEffect.Speed(speedDuration, speedAmount);
            owner.statusEffect.ApplyStatusEffect(speed, null);
            // TODO better
        }
        else
        {
            speed.SetDuration(speedDuration);
        }
    }
}
