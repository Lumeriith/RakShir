using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;

public enum IndicatorType { None, Range, Arrow }
[System.Serializable]
public class Indicator
{
    public IndicatorType type;
    public float range;
    public float arrowWidth;
    public float arrowLength;
    public bool enableSecondRangeIndicator = false;
    public float secondRange = 0.35f;
}

public enum AbilityInstanceEventTargetType { EveryInstance, FirstInstance, LastInstance };

public abstract class AbilityTrigger : MonoBehaviour
{
    public const int maxGemPerTrigger = 3;
    public enum TargetingType { None, PointStrict, PointNonStrict, Direction, Target }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {

        if (targetingType == TargetingType.PointNonStrict || targetingType == TargetingType.PointStrict || targetingType == TargetingType.Target)
        {
            UnityEditor.Handles.color = new Color(1, 0, 0, 1);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, range);
        }

        if (indicator != null && indicator.type == IndicatorType.Range)
        {
            UnityEditor.Handles.color = new Color(1, 0, 0, 1);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, indicator.range);
        }
        if (indicator != null && indicator.type == IndicatorType.Arrow)
        {
            UnityEditor.Handles.color = new Color(0, 1, 0, 1);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, indicator.arrowLength);
        }



    }
#endif
    [Header("Metadata Settings")]
    public Sprite abilityIcon;
    public string abilityName;
    [MultiLineProperty]
    public string abilityDescription;
    [Header("Effect Settings")]
    public GameObject[] soundEffect;
    public AnimationClip[] castAnimation;
    public float animationDuration;
    public Indicator indicator = new Indicator();
    [Header("Trigger Settings")]
    public bool dontCancelBasicCommands = false;
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

    public List<Gem> connectedGems;



    protected SourceInfo source
    {
        get
        {
            return new SourceInfo { trigger = this, thing = owner };
        }
    }

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

    public void Cast(CastInfo info, float animationDurationMultiplier = 1f)
    {
        if(soundEffect != null && soundEffect.Length != 0)
        {
            SFXManager.CreateSFXInstance(soundEffect[Random.Range(0, soundEffect.Length)].name, owner.transform.position);
        }

        if (castAnimation != null && castAnimation.Length != 0)
        {
            owner.PlayCustomAnimation(castAnimation[Random.Range(0, castAnimation.Length)], animationDuration * animationDurationMultiplier);
        }
        this.info = info;
        for(int i = 0; i < connectedGems.Count; i++)
        {
            connectedGems[i].photonView.RPC("RpcOnTriggerCast", RpcTarget.Others);
            connectedGems[i].OnTriggerCast(true);
        }
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
        SourceInfo source = new SourceInfo { trigger = this, thing = owner };
        return owner.SpendMana(manaCost, source);
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
        SourceInfo source = new SourceInfo() { trigger = this, thing = owner };
        PurgeInstancesList();
        if (info.owner == null) info.owner = owner;
        AbilityInstance instance = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, source, data);
        instances.Add(instance);
    }

    public void CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo info, object[] data = null)
    {
        SourceInfo source = new SourceInfo() { trigger = this, thing = owner };
        PurgeInstancesList();
        AbilityInstance instance = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, source, data);
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