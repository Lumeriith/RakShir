using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_JoyOfHunting : AbilityTrigger
{
    public float cooldownReductionAmount = 0.5f;
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_JoyOfHunting", transform.position, Quaternion.identity);
        StartCooldown();
        SpendMana();
    }

    public override bool CanBeCast()
    {
        return !IsAnyInstanceActive();
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
        ApplyCooldownReduction(cooldownReductionAmount);
    }
}
