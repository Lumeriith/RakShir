using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_RaiseShield : AbilityTrigger
{

    public int resetBasicAttackCount = 10;
    private int currentBasicAttackCount;

    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_RaiseShield", info.owner.transform.position, info.directionQuaternion);
        SpendMana();
        StartCooldown();
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit += BasicAttackHit;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit -= BasicAttackHit;
    }

    public override void AliveUpdate(bool isMine)
    {
        if (isMine) SetSpecialFillAmount(currentBasicAttackCount / resetBasicAttackCount);
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        currentBasicAttackCount++;
        if(resetBasicAttackCount <= currentBasicAttackCount)
        {
            if (isCooledDown) currentBasicAttackCount--;
            else
            {
                currentBasicAttackCount = 0;
                ResetCooldown();
            }
        }
    }
}
