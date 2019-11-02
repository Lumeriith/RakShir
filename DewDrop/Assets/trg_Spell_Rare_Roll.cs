using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Roll : AbilityTrigger
{
    public float cooldownReduction = 3f;
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_Roll", transform.position, info.directionQuaternion);
        StartCooldown();
        SpendMana();
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
        ApplyCooldownReduction(cooldownReduction);
    }

}
