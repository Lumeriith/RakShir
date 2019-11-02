using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public enum IndicatorType { None, Range, Arrow }
[System.Serializable]
public class Indicator
{
    public IndicatorType type;
    public float range;
    public float arrowWidth;
    public float arrowLength;

}

public enum AbilityInstanceEventTargetType { EveryInstance, FirstInstance, LastInstance };

public abstract class AbilityTrigger : MonoBehaviour
{
    public enum TargetingType { None, PointStrict, PointNonStrict, Direction, Target }

    

    [Header("Metadata Settings")]
    public Sprite abilityIcon;
    public string abilityName;
    [ResizableTextArea]
    public string abilityDescription;
    [Header("Visual Settings")]
    public AnimationClip[] castAnimation;
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

    private float specialFillAmount = 0;
    private List<AbilityInstance> instances = new List<AbilityInstance>();


    public float cooldownTime;

    public int skillIndex
    {
        get
        {
            if(_skillIndex == -1)
            {
                for(int i = 0; i < owner.control.skillSet.Length; i++)
                {
                    if (owner.control.skillSet[i] == this) _skillIndex = i;
                }
            }
            return _skillIndex;
        }
        set
        {
            _skillIndex = value;
        }
    }
    private int _skillIndex = -1;

    [HideInInspector]
    public LivingThing owner
    {
        get
        {
            if (_owner == null) _owner = GetComponentInParent<LivingThing>();
            return _owner;
        }
        set
        {
            _owner = value;
        }
    }
    private LivingThing _owner;

    [HideInInspector]
    public CastInfo info;

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
        if (castAnimation != null && castAnimation.Length != 0)
        {
            if (owner.control.skillSet[0] != null && owner.control.skillSet[0] == this)
            {
                float duration = (1 / owner.stat.finalAttacksPerSecond) / ((100 + owner.statusEffect.totalHasteAmount) / 100f) + 0.05f;
                owner.PlayCustomAnimation(castAnimation[Random.Range(0, castAnimation.Length)], duration * animationDuration);
            }
            else
            {
                owner.PlayCustomAnimation(castAnimation[Random.Range(0, castAnimation.Length)], animationDuration);
            }

        }
        this.info = info;
        OnCast(info);

    }

    public abstract void OnCast(CastInfo info);

    public virtual void OnEquip() { }

    public virtual void AliveUpdate(bool isMine) { }

    private void Update()
    {
        if(_owner != null)
        {
            AliveUpdate(_owner.photonView.IsMine);
        }
    }

    public virtual void OnUnequip() { }

    public virtual bool IsReady() { return true; }
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
        }
    }

    public void StartCooldown(float time)
    {

        owner.control.cooldownTime[skillIndex] = time;
    }

    public void SetSpecialFillAmount(float amount)
    {
        specialFillAmount = Mathf.Clamp(amount, 0f, 1f);
    }
    public float GetSpecialFillAmount()
    {
        return specialFillAmount;
    }


    public bool SpendMana()
    {
        return owner.SpendMana(manaCost);
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

    public void CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, object[] data = null)
    {
        PurgeInstancesList();
        AbilityInstance instance = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, data);
        instances.Add(instance);
    }

    public void CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo info, object[] data = null)
    {
        PurgeInstancesList();
        AbilityInstance instance = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, data);
        instances.Add(instance);
    }

    private void PurgeInstancesList()
    {
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if (instances[i] == null || !instances[i].isAlive)
            {
                instances.RemoveAt(i);
            }
        }
    }
    
    public bool IsAnyInstanceActive()
    {
        PurgeInstancesList();
        return instances.Count != 0;
    }

    public AbilityInstance GetLastInstance()
    {
        PurgeInstancesList();
        if (instances.Count == 0) return null;
        return instances[instances.Count - 1];
    }

    public AbilityInstance GetFirstInstsance()
    {
        PurgeInstancesList();
        if (instances.Count == 0) return null;
        return instances[0];
    }



    public void SendEventToAbilityInstance(string eventString, AbilityInstanceEventTargetType target)
    {
        if (!IsAnyInstanceActive())
        {
            Debug.LogWarning("There is no active AbilityInstance to send event to!\n" + name);
            return;
        }
        switch (target)
        {
            case AbilityInstanceEventTargetType.EveryInstance:
                for(int i = 0; i < instances.Count; i++)
                {
                    instances[i].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                }
                break;
            case AbilityInstanceEventTargetType.FirstInstance:
                instances[0].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
            case AbilityInstanceEventTargetType.LastInstance:
                instances[instances.Count - 1].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
        }


    }
}