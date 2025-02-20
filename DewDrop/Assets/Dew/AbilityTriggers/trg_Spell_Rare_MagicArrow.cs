﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_MagicArrow : AbilityTrigger
{
    public int shotArrows = 0;
    public float cooldownReductionAmount = 0.5f;

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
        ApplyCooldownReduction(cooldownReductionAmount);
    }


    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.2f, false, false, false, false, ChannelFinish, null));
        StartCooldown();
        SpendMana();
    }

    public override void AliveUpdate(bool isMine)
    {
        if (isMine) SetSpecialFillAmount(shotArrows / 2f);
    }

    private void ChannelFinish()
    {
        info.owner.control.LookAt(info.owner.transform.position + info.directionVector, true);
        if (++shotArrows >= 3)
        {
            CreateAbilityInstance("ai_Spell_Rare_MagicArrow", info.owner.transform.position + info.owner.GetCenterOffset() * 1.55f + info.directionVector, info.directionQuaternion, new object[] { true });
            SFXManager.CreateSFXInstance("si_Spell_Rare_MagicArrow EmpoweredCast", info.owner.transform.position);
            shotArrows = 0;
        }
        else
        {
            CreateAbilityInstance("ai_Spell_Rare_MagicArrow", info.owner.transform.position + info.owner.GetCenterOffset() * 1.55f + info.directionVector, info.directionQuaternion, new object[] { false });
            SFXManager.CreateSFXInstance("si_Spell_Rare_MagicArrow Cast", info.owner.transform.position);
        }
    }
}
