using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_IHaveManyDaggers : AbilityTrigger
{
    public float duration = 8f;
    private float startTime;
    private bool shouldCooldownStart = false;

    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_IHaveManyDaggers", info.owner.transform.position, Quaternion.identity);
        SpendMana();
        startTime = Time.time;
        shouldCooldownStart = true;
    }

    public override void AliveUpdate(bool isMine)
    {
        if (isMine)
        {
            if(Time.time - startTime > duration)
            {
                SetSpecialFillAmount(0f);
                if (shouldCooldownStart)
                {
                    StartCooldown();
                    shouldCooldownStart = false;
                }
            }
            else
            {
                SetSpecialFillAmount((duration - Time.time + startTime) / duration);
            }
        }
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine && shouldCooldownStart)
        {
            shouldCooldownStart = false;
            StartCooldown();
        }
    }

    public override bool IsReady()
    {
        return (startTime == 0 || Time.time - startTime > duration);
    }
}
