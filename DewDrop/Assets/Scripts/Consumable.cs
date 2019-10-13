using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
public abstract class Consumable : Item
{
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

    public void DestroySelf()
    {
        PlayerItemBelt belt = owner.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        Disown();
        PhotonNetwork.Destroy(gameObject);
    }

    public abstract bool OnUse(CastInfo info);

}
