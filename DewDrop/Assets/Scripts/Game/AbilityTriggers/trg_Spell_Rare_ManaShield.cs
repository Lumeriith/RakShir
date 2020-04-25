using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ManaShield : AbilityTrigger
{
    public float maximumShieldAmount = 300f;
    private float elapsedTime = 0f;
    private float manaSpendInterval = 1f;
    public override void OnCast(CastInfo info)
    {
        if (IsAnyInstanceActive())
        {
            SendEventToAbilityInstance("Off", AbilityInstanceEventTargetType.EveryInstance);
            StartCooldown();
        }
        else
        {
            CreateAbilityInstance("ai_Spell_Rare_ManaShield", transform.position, Quaternion.identity);
            StartCooldown();
            SpendMana();
        }
    }

    public override void OnUnequip()
    {
        if(owner.photonView.IsMine && IsAnyInstanceActive())
        {
            SendEventToAbilityInstance("Off", AbilityInstanceEventTargetType.EveryInstance);
        }
    }

    public override void AliveUpdate(bool isMine)
    {
        if (!isMine) return;
        ai_Spell_Rare_ManaShield ai = GetLastInstance() as ai_Spell_Rare_ManaShield;
        if (ai != null)
        {
            SetSpecialFillAmount((float)ai.shield.parameter / (maximumShieldAmount / 100f * owner.stat.finalSpellPower));
            elapsedTime += Time.deltaTime;
            if (elapsedTime > manaSpendInterval)
            {
                elapsedTime = 0f;
                if (info.owner.HasMana(manaCost))
                {
                    SpendMana();
                }
                else
                {
                    SendEventToAbilityInstance("Off", AbilityInstanceEventTargetType.EveryInstance);
                }

            }
        }
        else
        {
            SetSpecialFillAmount(0f);
        }
    }

}
