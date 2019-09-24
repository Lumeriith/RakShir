using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public abstract class SpellTrigger : MonoBehaviour
{
    public enum TargetingType { None, PointStrict, PointNonStrict, Direction, Target }


    [Header("Trigger Settings")]
    public TargetingType targetingType;
    [ShowIf("ShouldTargetMaskFieldShow")]
    public LayerMask targetMask;
    [ShowIf("ShouldRangeFieldShow")]
    public float range;

    public float cooldownTime;


    private LivingThing livingThing
    {
        get
        {
            if(_livingThing == null)
            {
                _livingThing = transform.parent.GetComponent<LivingThing>();
            }
            return _livingThing;
        }
    }
    private LivingThing _livingThing;
    public bool isCooledDown
    {
        get
        {
            return remainingCooldownTime == 0;
        }
    }



    [ShowNativeProperty]
    public float remainingCooldownTime
    {
        get
        {
            return Mathf.Max(cooldownTime - (Time.time - cooldownStartTime), 0);
        }
    }


    private float cooldownStartTime = 0;

    public abstract void OnCast(SpellManager.CastInfo info);

    protected bool ShouldTargetMaskFieldShow()
    {
        return targetingType == TargetingType.Target;
    }

    protected bool ShouldRangeFieldShow()
    {
        return targetingType == TargetingType.Target || targetingType == TargetingType.PointStrict || targetingType == TargetingType.PointNonStrict;
    }

    public void StartCooldown()
    {
        cooldownStartTime = Time.time;
        if(livingThing != null &&
           livingThing.control.basicAttackSpellTrigger != null &&
           livingThing.control.basicAttackSpellTrigger == this &&
           livingThing.stat != null)
        {
            ApplyCooldownReduction(cooldownTime * (1 - (1 / (1 + livingThing.stat.fi))));
        }
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
