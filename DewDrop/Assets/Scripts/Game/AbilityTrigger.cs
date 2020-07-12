using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;


public enum CastMethodType { None, Cone, Arrow, Target, Point }

/// <summary>
/// Data container for describing and serializing a cast method for an AbilityTrigger.
/// </summary>
[System.Serializable]
public class CastMethodData
{
    [HideLabel, EnumToggleButtons]
    public CastMethodType type;
    [HideLabel, BoxGroup]
    public SelfValidator selfValidator;
    [HideLabel, BoxGroup]
    public TargetValidator targetValidator;
    [ShowIf(nameof(ShouldAngleFieldShow))]
    public float angle;
    [ShowIf(nameof(ShouldLengthFieldShow))]
    public float length;
    [ShowIf(nameof(ShouldWidthFieldShow))]
    public float width;
    [ShowIf(nameof(ShouldRangeFieldShow))]
    public float range;
    [ShowIf(nameof(ShouldRadiusFieldShow))]
    public float radius;
    [ShowIf(nameof(ShouldIsClampingFieldShow))]
    public bool isClamping;

    private bool ShouldAngleFieldShow => type == CastMethodType.Cone;
    private bool ShouldLengthFieldShow => type == CastMethodType.Arrow;
    private bool ShouldWidthFieldShow => type == CastMethodType.Arrow;
    private bool ShouldRangeFieldShow => type == CastMethodType.Target || type == CastMethodType.Point;
    private bool ShouldRadiusFieldShow => type == CastMethodType.None || type == CastMethodType.Cone || type == CastMethodType.Target || type == CastMethodType.Point;
    private bool ShouldIsClampingFieldShow => type == CastMethodType.Point;
}

public enum AbilityInstanceEventTargetType { EveryInstance, FirstInstance, LastInstance };

public abstract class AbilityTrigger : MonoBehaviour, ICastable
{
    public const int maxGemPerTrigger = 3;
    public Equipment equipment
    {
        get
        {
            if (_equipment == null) _equipment = GetComponentInParent<Equipment>();
            return _equipment;
        }
    }
    private Equipment _equipment;

    [BoxGroup("Trigger Metadata"), HorizontalGroup("Trigger Metadata/horizontal", 50), VerticalGroup("Trigger Metadata/horizontal/icon")]
    [PreviewField(50, ObjectFieldAlignment.Left)]
    [HideLabel]
    public Sprite abilityIcon;
    [HorizontalGroup("Trigger Metadata/horizontal"), VerticalGroup("Trigger Metadata/horizontal/text")]
    [HideLabel]
    public string abilityName = "Ability Name";
    [HorizontalGroup("Trigger Metadata/horizontal"), VerticalGroup("Trigger Metadata/horizontal/text")]
    public float manaCost;
    [HorizontalGroup("Trigger Metadata/horizontal"), VerticalGroup("Trigger Metadata/horizontal/text")]
    public float cooldownTime;

    [HideLabel]
    [MultiLineProperty(6)]
    [BoxGroup("Metadata")]
    public string abilityDescription = "This is an awesome ability!";

    [Header("Trigger Settings")]
    public CastMethodData castMethod = new CastMethodData();
    public SelfValidator selfValidator => castMethod.selfValidator;
    public TargetValidator targetValidator => castMethod.targetValidator;


    [Header("Effect Settings")]
    public GameObject[] soundEffect;
    public AnimationClip[] castAnimation;
    public float animationDuration = 1f;
    public Ease timeCurve = Ease.Linear;



    private float specialFillAmount = 0;
    private List<AbilityInstance> _instances = new List<AbilityInstance>();




    public List<Gem> connectedGems;


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
    public Entity owner
    {
        get
        {
            if (_owner == null) _owner = GetComponentInParent<Entity>();
            return _owner;
        }
        set
        {
            _owner = value;
        }
    }
    private Entity _owner;

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
        Cast(info, 1f);
    }

    public void Cast(CastInfo info, float animationDurationMultiplier)
    {
        if (soundEffect != null && soundEffect.Length != 0)
        {
            SFXManager.CreateSFXInstance(soundEffect[Random.Range(0, soundEffect.Length)].name, owner.transform.position);
        }

        if (castAnimation != null && castAnimation.Length != 0)
        {
            owner.PlayCustomAnimation(castAnimation[Random.Range(0, castAnimation.Length)], animationDuration * animationDurationMultiplier, timeCurve);
        }
        this.info = info;
        for (int i = 0; i < connectedGems.Count; i++)
        {
            connectedGems[i].photonView.RPC("RpcOnTriggerCast", RpcTarget.Others);
            connectedGems[i].OnTriggerCast(true);
        }
        OnCast(info);
    }


    public abstract void OnCast(CastInfo info);

    public virtual void OnEquip() { }

    public virtual void AliveUpdate(bool isMine) { }

    private void Awake()
    {
        _equipment = GetComponentInParent<Equipment>();
    }

    private void Update()
    {
        if(_owner != null)
        {
            AliveUpdate(_owner.photonView.IsMine);
        }
    }

    public virtual void OnUnequip() { }

    /// <summary>
    /// Can this AbilityTrigger be cast? Always returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBeCast() => true;

    public void StartCooldown(bool isBasicAttack = false)
    {
        if (isBasicAttack)
        {

            owner.control.cooldownTime[skillIndex] = 1 / owner.stat.finalAttacksPerSecond;

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
        return owner.SpendMana(manaCost, null);
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

    public AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, object[] data = null)
    {
        PurgeInstancesList();
        if (info.owner == null) info.owner = owner;
        return CreateAbilityInstance(prefabName, position, rotation, info, data);
    }

    public AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo info, object[] data = null)
    {
        PurgeInstancesList();
        AbilityInstance instance = AbilityInstanceManager.CreateAbilityInstance(prefabName, position, rotation, info, data);
        _instances.Add(instance);
        for(int i = 0; i < connectedGems.Count; i++)
        {
            connectedGems[i].photonView.RPC("RpcOnAbilityInstanceCreatedFromTrigger", RpcTarget.Others, instance.photonView.ViewID);
            connectedGems[i].OnAbilityInstanceCreatedFromTrigger(true, instance);
        }
        return instance;
    }

    private void PurgeInstancesList()
    {
        for (int i = _instances.Count - 1; i >= 0; i--)
        {
            if (!_instances[i].isAlive)
            {
                _instances.RemoveAt(i);
            }
        }
    }
    
    public bool IsAnyInstanceActive()
    {
        PurgeInstancesList();
        return _instances.Count != 0;
    }

    public AbilityInstance GetLastInstance()
    {
        PurgeInstancesList();
        if (_instances.Count == 0) return null;
        return _instances[_instances.Count - 1];
    }

    public AbilityInstance GetFirstInstance()
    {
        PurgeInstancesList();
        if (_instances.Count == 0) return null;
        return _instances[0];
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
                for(int i = 0; i < _instances.Count; i++)
                {
                    _instances[i].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                }
                break;
            case AbilityInstanceEventTargetType.FirstInstance:
                _instances[0].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
            case AbilityInstanceEventTargetType.LastInstance:
                _instances[_instances.Count - 1].photonView.RPC("RpcDoEvent", RpcTarget.All, eventString);
                break;
        }
    }

    void ICastable.Cast(CastInfo info) => Cast(info);

    bool ICastable.IsReady() => isCooledDown && owner.HasMana(manaCost) && castMethod.selfValidator.Evaluate(owner) && CanBeCast();

    bool ICastable.IsCastValid(CastInfo info)
    {
        if (castMethod.type != CastMethodType.Target) return true;
        else return info.target != null && castMethod.targetValidator.Evaluate(info.owner, info.target);
    }

    CastMethodData ICastable.castMethod => castMethod;
}