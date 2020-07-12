using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;
public abstract class Consumable : Item, ICastable
{
    [Header("Consumable Settings")]
    public bool useOnPickup = false;
    public AnimationClip useAnimation;
    public float animationDuration;

    [Header("Trigger Settings")]
    public CastMethodData castMethod;

    public void Cast(CastInfo info)
    {
        owner.PlayCustomAnimation(useAnimation, animationDuration);
        OnUse(info);
    }

    /// <summary>
    /// Can this Consumable be cast? Always returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeCast() => true;

    public abstract void OnUse(CastInfo info);

    void ICastable.Cast(CastInfo info) => Cast(info);

    bool ICastable.IsReady() => castMethod.selfValidator.Evaluate(owner) && CanBeCast();

    bool ICastable.IsCastValid(CastInfo info) => castMethod.targetValidator.Evaluate(info.owner, info.target);

    public override InfoTextIcon infoTextIcon => InfoTextIcon.Consumable;

    CastMethodData ICastable.castMethod => castMethod;
}
