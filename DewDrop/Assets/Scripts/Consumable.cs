using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public abstract class Consumable : Activatable
{
    [Header("Description Settings")]
    public Sprite sprite;
    public string consumableName;
    [Multiline]
    public string description;


    [Header("Consumable Settings")]
    public bool useOnPickup = false;
    public AnimationClip useAnimation;
    public float animationDuration;
    public Indicator indicator = new Indicator();

    [Header("Trigger Settings")]
    public AbilityTrigger.TargetingType targetingType;
    [ShowIf("ShouldRangeFieldShow")]
    public float range;
    [ShowIf("ShouldTargetValidatorFieldShow")]
    public TargetValidator targetValidator;
    public SelfValidator selfValidator;

    [HideInInspector]
    public LivingThing owner = null;

    protected bool ShouldTargetValidatorFieldShow()
    {
        return targetingType == AbilityTrigger.TargetingType.Target;
    }

    protected bool ShouldRangeFieldShow()
    {
        return targetingType == AbilityTrigger.TargetingType.Target || targetingType == AbilityTrigger.TargetingType.PointStrict || targetingType == AbilityTrigger.TargetingType.PointNonStrict;
    }

    public bool Use(CastInfo info)
    {
        bool isUsed = OnUse(info);
        if (useAnimation != null && isUsed)
        {
            owner.PlayCustomAnimation(useAnimation, animationDuration);
        }

        return isUsed;
    }

    public abstract bool OnUse(CastInfo info);

    protected override void OnChannelSuccess(LivingThing activator)
    {
        PlayerItemBelt belt = activator.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (owner != null) return;
        owner = activator;
        if (belt.AddConsumable(this))
        {
            transform.SetParent(activator.transform);
            transform.position = activator.transform.position;
            gameObject.SetActive(false);
        }
    }

    protected override void OnChannelCancel(LivingThing activator)
    {
        
    }

    protected override void OnChannelStart(LivingThing activator)
    {

    }

}
