using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum IndicatorType { None, Range, Arrow }
[System.Serializable]
public class Indicator
{
    public IndicatorType type;
    public float range;
    public float arrowWidth;
    public float arrowLength;

}

public abstract class AbilityTrigger : MonoBehaviour
{
    public enum TargetingType { None, PointStrict, PointNonStrict, Direction, Target }

    [Header("UI Settings")]
    public Sprite abilityIcon;
    public string abilityName;
    [ResizableTextArea]
    public string abilityDescription;
    [Header("Visual Settings")]
    public AnimationClip castAnimation;
    public float animationDuration;
    public Indicator indicator = new Indicator();
    [Header("Trigger Settings")]
    public TargetingType targetingType;
    [ShowIf("ShouldRangeFieldShow")]
    public float range;
    public float manaCost;
    [ShowIf("ShouldTargetValidatorFieldShow")]
    public TargetValidator targetValidator;
    public SelfValidator selfValidator;

    

    public float cooldownTime;

    [HideInInspector]
    public int skillIndex;

    [HideInInspector]
    public LivingThing owner;
    public bool isCooledDown
    {
        get
        {
            return remainingCooldownTime == 0;
        }
    }


    public float remainingCooldownTime
    {
        get
        {
            return owner.control.cooldownTime[skillIndex];
        }
    }

    public void Cast(CastInfo info)
    {
        if (castAnimation != null)
        {
            if (owner.control.skillSet[0] != null && owner.control.skillSet[0] == this)
            {
                float duration = (1 / owner.stat.finalAttacksPerSecond) / ((100 + owner.statusEffect.totalHasteAmount) / 100f) + 0.05f;
                owner.PlayCustomAnimation(castAnimation, duration * animationDuration);
            }
            else
            {
                owner.PlayCustomAnimation(castAnimation, animationDuration);
            }

        }
        OnCast(info);

    }

    public abstract void OnCast(CastInfo info);

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }


    protected bool ShouldTargetValidatorFieldShow()
    {
        return targetingType == TargetingType.Target;
    }

    protected bool ShouldRangeFieldShow()
    {
        return targetingType == TargetingType.Target || targetingType == TargetingType.PointStrict || targetingType == TargetingType.PointNonStrict;
    }

    public void StartCooldown(bool isBasicAttack = false)
    {
        if (isBasicAttack)
        {

            owner.control.cooldownTime[skillIndex] = (1 / owner.stat.finalAttacksPerSecond) / ((100 + owner.statusEffect.totalHasteAmount) / 100f);

        }
        else
        {
            owner.control.cooldownTime[skillIndex] = cooldownTime;
            ApplyCooldownReduction(cooldownTime * owner.stat.finalCooldownReduction / 100f);
        }
    }

    public void StartCooldown(float time, bool ignoreCooldownReduction = false)
    {

        owner.control.cooldownTime[skillIndex] = time;
        if (!ignoreCooldownReduction) ApplyCooldownReduction(time * owner.stat.finalCooldownReduction / 100f);
    }



    public void RefundMana()
    {
        owner.stat.currentMana += manaCost;
        owner.stat.ValidateMana();
        owner.stat.SyncChangingStats();
    }

    public void ResetCooldown()
    {
        owner.control.cooldownTime[skillIndex] = 0f;
    }

    public void ApplyCooldownReduction(float time)
    {
        owner.control.cooldownTime[skillIndex] = Mathf.MoveTowards(owner.control.cooldownTime[skillIndex], 0, time);
    }



}
