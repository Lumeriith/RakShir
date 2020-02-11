using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Annihilation : AbilityTrigger
{
    public TargetValidator affectedTargets;
    public float auraRadius = 7f;

    public float auraDuration = 8.1f;

    private float currentDuration;

    public override void OnCast(CastInfo info)
    {
        SpendMana();
        StartCooldown();
        CreateAbilityInstance("ai_Spell_Rare_Annihilation", transform.position, Quaternion.identity);
        currentDuration = auraDuration;
        StartCoroutine("CoroutineTick");
        SetSpecialFillAmount(currentDuration / auraDuration);
    }

    private IEnumerator CoroutineTick()
    {
        while(currentDuration > 0)
        {
            yield return new WaitForSeconds(0.25f);
            if (owner.GetAllTargetsInRange(owner.transform.position, auraRadius, affectedTargets).Count == 0)
            {
                currentDuration -= 0.25f;
            }
            SetSpecialFillAmount(currentDuration / auraDuration);
        }
        SendEventToAbilityInstance("EndAura", AbilityInstanceEventTargetType.EveryInstance);
        SetSpecialFillAmount(0f);
    }

    public override bool IsReady()
    {
        return !IsAnyInstanceActive();
    }
}
