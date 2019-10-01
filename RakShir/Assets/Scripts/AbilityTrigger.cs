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

    



    public LivingThing owner
    {
        get
        {
            if(_owner == null)
            {
                _owner = transform.parent.GetComponent<LivingThing>();
            }
            return _owner;
        }
    }
    private LivingThing _owner;
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
            return Mathf.Max(cooldownTime - (Time.time - cooldownStartTime), 0);
        }
    }


    private float cooldownStartTime = -10000;

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
            cooldownStartTime = Time.time - cooldownTime + (1 / owner.stat.finalAttacksPerSecond) / ((100 + owner.statusEffect.totalHasteAmount) / 100f);
        
        }
        else
        {
            cooldownStartTime = Time.time;
            ApplyCooldownReduction(cooldownTime * owner.stat.finalCooldownReduction / 100f);
        }
    }

    public void RefundMana()
    {
        owner.stat.currentMana += manaCost;
        owner.stat.ValidateMana();
        owner.stat.SyncChangingStats();
    }

    public void SetCooldown(float time)
    {
        cooldownStartTime = time + Time.time - cooldownTime;
    }

    public void ResetCooldown()
    {
        cooldownStartTime = Time.time - cooldownTime;
    }

    public void ApplyCooldownReduction(float time)
    {
        cooldownStartTime -= time;
    }



}
