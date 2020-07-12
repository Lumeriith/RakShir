using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Hamstring : AbilityTrigger
{
    private bool isHamstringActive = false;
    private float hamstringActiveTime;

    public float hamstringActiveDuration = 3f;


    public override void OnCast(CastInfo info)
    {
        hamstringActiveTime = Time.time;
        isHamstringActive = true;
        CreateAbilityInstance("ai_Spell_Rare_HamstringBuff", transform.position, Quaternion.identity);
        SpendMana();
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine) owner.OnDoBasicAttackHit += BasicAttackHit;
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDoBasicAttackHit -= BasicAttackHit;
            if (isHamstringActive)
            {
                SendEventToAbilityInstance("RemoveBuff", AbilityInstanceEventTargetType.EveryInstance);
                isHamstringActive = false;
                SetSpecialFillAmount(0f);
                StartCooldown();
            }
        }
    }

    private void BasicAttackHit(InfoBasicAttackHit info)
    {
        if (!isHamstringActive) return;
        CastInfo castInfo = new CastInfo();
        castInfo.owner = this.info.owner;
        castInfo.target = info.to;
        CreateAbilityInstance("ai_Spell_Rare_Hamstring", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, castInfo);
        SendEventToAbilityInstance("RemoveBuff", AbilityInstanceEventTargetType.EveryInstance);
        isHamstringActive = false;
        SetSpecialFillAmount(0f);
        StartCooldown();
    }

    public override bool CanBeCast()
    {
        return !isHamstringActive;
    }

    public override void AliveUpdate(bool isMine)
    {
        if (isMine)
        {
            if (isHamstringActive)
            {
                if(Time.time - hamstringActiveTime > 3f)
                {
                    SendEventToAbilityInstance("RemoveBuff", AbilityInstanceEventTargetType.EveryInstance);
                    isHamstringActive = false;
                    SetSpecialFillAmount(0f);
                    StartCooldown();
                }
                else
                {
                    SetSpecialFillAmount((hamstringActiveDuration - Time.time + hamstringActiveTime) / hamstringActiveDuration);
                }
            }
        }
    }


}
