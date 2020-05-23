using Photon.Pun;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

#region Enums
public enum Team { None, Red, Blue, Creep }
public enum LivingThingType { Monster, Player, Summon }

public enum LivingThingTier { None, Lesser, Normal, Elite, Boss }


public enum DamageType { Physical, Spell, Pure }
public enum Relation { Own, Enemy, Ally }



#endregion Enums



[RequireComponent(typeof(EntityControl))]
[RequireComponent(typeof(EntityStat))]
[RequireComponent(typeof(EntityStatusEffect))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformViewClassic))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Entity : MonoBehaviourPun
{
    private bool didDamageFlash;

    private NavMeshAgent _agent;
    private Entity _lastAttacker;
    private Animator _animator;
    private AnimatorOverrideController _overrideController;
    private AnimationClip[] _defaultClips;

    private List<Color> _flashColors = new List<Color>();
    private List<float> _flashDurations = new List<float>();

    private Transform _model;
    private Vector3 _defaultScale;

    private List<float> _scaleMultipliers = new List<float>();
    private List<float> _scaleDurations = new List<float>();

    private List<Material> _materials = new List<Material>();
    private List<Color> _defaultEmissionColors = new List<Color>();
    private List<Texture> _defaultEmissionMaps = new List<Texture>();
    private List<bool> _defaultKeywordEnabled = new List<bool>();

    [HideInInspector]
    public Displacement ongoingDisplacement = null;

    private float _defaultMovementSpeed;

    private float _customAnimationLinearNormalizedTime = 0f;
    private float _customAnimationDuration = 0f;
    private EaseFunction _customAnimationEaseMethod;


    public static Entity GetFromViewID(int viewID)
    {
        return PhotonNetwork.GetPhotonView(viewID).GetComponent<Entity>();
    }

    #region Action Declarations
    public System.Action<InfoAbilityInstance> OnAbilityInstanceCreated = (_) => { };

    public System.Action<InfoDamage> OnDealDamage = (InfoDamage _) => { };
    public System.Action<InfoDamage> OnTakeDamage = (InfoDamage _) => { };

    public System.Action<InfoDamage> OnDealPureDamage = (InfoDamage _) => { };
    public System.Action<InfoDamage> OnTakePureDamage = (InfoDamage _) => { };

    public System.Action<InfoMagicDamage> OnDealMagicDamage = (InfoMagicDamage _) => { };
    public System.Action<InfoMagicDamage> OnTakeMagicDamage = (InfoMagicDamage _) => { };

    public System.Action<InfoGold> OnTakeGold = (InfoGold _) => { };
    public System.Action<InfoGold> OnGiveGold = (InfoGold _) => { };

    public System.Action<InfoSpendGold> OnSpendGold = (InfoSpendGold _) => { };

    public System.Action<InfoBasicAttackHit> OnDoBasicAttackHit = (InfoBasicAttackHit _) => { };
    public System.Action<InfoBasicAttackHit> OnTakeBasicAttackHit = (InfoBasicAttackHit _) => { };


    public System.Action<InfoMiss> OnDodge = (InfoMiss _) => { };
    public System.Action<InfoMiss> OnMiss = (InfoMiss _) => { };

    public System.Action<InfoDeath> OnDeath = (InfoDeath _) => { };
    public System.Action<InfoDeath> OnKill = (InfoDeath _) => { };

    public System.Action<InfoHeal> OnDoHeal = (InfoHeal _) => { };
    public System.Action<InfoHeal> OnTakeHeal = (InfoHeal _) => { };

    public System.Action<InfoManaHeal> OnDoManaHeal = (InfoManaHeal _) => { };
    public System.Action<InfoManaHeal> OnTakeManaHeal = (InfoManaHeal _) => { };


    public System.Action OnStartStunned = () => { };
    public System.Action OnStopStunned = () => { };

    public System.Action<InfoManaSpent> OnSpendMana = (InfoManaSpent _) => { };

    #endregion Action Declarations

    #region NaughtyAttributes

    bool ShouldShowSummonerField()
    {
        return type == LivingThingType.Summon;
    }

    #endregion NaughtyAttributes

    #region References For Everyone
    public string readableName;

    public Team team = Team.None;
    public LivingThingType type = LivingThingType.Monster;
    public LivingThingTier tier = LivingThingTier.None;

    public float droppedGold = 10f;


    public Room currentRoom {
        get
        {
            if(_currentRoom == null)
            {
                UpdateCurrentRoom();
            }
            return _currentRoom;
        }
        set
        {
            _currentRoom = value;
        }

    }

    public bool isMine
    {
        get
        {
            return photonView.IsMine;
        }
    }

    private Room _currentRoom;

    private GameObject unitBase;

    [ShowIf("ShouldShowSummonerField")]
    public Entity summoner = null;

    [Header("Animation Settings")]
    public AnimationClip defaultStand;
    public AnimationClip defaultWalk;
    public AnimationClip defaultStunned;
    public AnimationClip defaultDeath;

    [Header("Optional Explicit Transforms")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform top;
    public Transform bottom;

    [HideInInspector]
    public EntityControl control;
    [HideInInspector]
    public EntityStat stat;
    [HideInInspector]
    public EntityStatusEffect statusEffect;
    [HideInInspector]
    public MeshOutline outline;

    public float unitRadius { get; private set; }
    public float currentHealth
    {
        get
        {
            return stat.currentHealth;
        }
    }
    public float maximumHealth
    {
        get
        {
            return stat.finalMaximumHealth;
        }
    }
    [Header("Decay Settings")]
    public bool shouldDecay = true;
    public float startDecayTime = 20f;
    public float endDecayTime = 40f;
    private float timeOfDeath = 0f;

    #endregion References For Everyone

    #region Unity

    private void Awake()
    {
        ongoingDisplacement = null;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        _agent = GetComponent<NavMeshAgent>();
        _model = transform.Find("Model");

        _defaultScale = _model.localScale;
        _model.localScale = Vector3.zero;

        control = GetComponent<EntityControl>();
        stat = GetComponent<EntityStat>();
        statusEffect = GetComponent<EntityStatusEffect>();
        gameObject.layer = LayerMask.NameToLayer("LivingThing");

        _defaultMovementSpeed = stat.baseMovementSpeed;

        _animator = transform.Find("Model").GetComponent<Animator>();
        _animator.applyRootMotion = false;

        if (photonView.IsMine)
        {
            _agent.avoidancePriority++;
        }

        if (type == LivingThingType.Monster && tier == LivingThingTier.Boss)
        {
            _agent.avoidancePriority -= 5;
        }
        if (type == LivingThingType.Monster && tier == LivingThingTier.Elite)
        {
            _agent.avoidancePriority -= 2;
        }
        if (type == LivingThingType.Player) _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        _agent.stoppingDistance = 0.1f;
        _agent.autoRepath = false;
        _agent.autoBraking = true;
        _defaultClips = _animator.runtimeAnimatorController.animationClips;
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (AnimationClip oldClip in _defaultClips)
        {
            if (oldClip.name == "Stand")
            {
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, defaultStand));
                _overrideController.ApplyOverrides(overrides);
            }
            else if (oldClip.name == "Walk")
            {
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, defaultWalk));
                _overrideController.ApplyOverrides(overrides);
            }
            else if (oldClip.name == "Stunned")
            {
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, defaultStunned));
                _overrideController.ApplyOverrides(overrides);
            }
            else if (oldClip.name == "Death")
            {
                overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, defaultDeath));
                _overrideController.ApplyOverrides(overrides);
            }
        }
        _overrideController.ApplyOverrides(overrides);

        outline = gameObject.AddComponent<MeshOutline>();
        /*
        Renderer renderer = transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            outline = renderer.gameObject.AddComponent<MeshOutline>();
        }
        else
        {
            outline = transform.Find("Model").gameObject.AddComponent<MeshOutline>();
        }
        */

        if(type == LivingThingType.Player)
        {
            outline.OutlineMode = MeshOutline.Mode.SilhouetteOnly;
        }
        else
        {
            outline.enabled = false;
        }
        

        AssignMissingTransforms();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].material.HasProperty("_EmissionColor") ||
                !renderers[i].material.HasProperty("_EmissionMap")) continue;
            _materials.Add(renderers[i].material);
            _defaultEmissionColors.Add(_materials[_materials.Count - 1].GetColor("_EmissionColor"));
            _defaultEmissionMaps.Add(_materials[_materials.Count - 1].GetTexture("_EmissionMap"));
            _defaultKeywordEnabled.Add(_materials[_materials.Count - 1].IsKeywordEnabled("_EMISSION"));
        }

        OnTakeDamage += (InfoDamage _) => {
            if (GameManager.instance.localPlayer == null || GameManager.instance.localPlayer != this)
            {
                if (didDamageFlash) return;
                didDamageFlash = true;
                RpcFlashForDuration(1, 1, 1, 1, 0.5f, 0.10f);
                RpcFlashForDuration(1, 1, 1, 1, 0.5f, 0.06f);
                RpcFlashForDuration(1, 1, 1, 1, 0.5f, 0.02f);
            }
        };
    }



    private void Start()
    {
        GameManager.instance.OnLivingThingInstantiate.Invoke(this);
        unitRadius = GetComponent<CapsuleCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }

    
    private void Update()
    {
        UpdateCustomAnimationNormalizedTime();

        if (unitBase == null && GameManager.instance.localPlayer != null)
        {
            GameObject basePrefab;
            Relation relation = GameManager.instance.localPlayer.GetRelationTo(this);
            if (relation == Relation.Ally) basePrefab = Resources.Load<GameObject>("Ally Base");
            else if (relation == Relation.Enemy) basePrefab = Resources.Load<GameObject>("Enemy Base");
            else basePrefab = Resources.Load<GameObject>("Self Base");
            unitBase = Instantiate(basePrefab, transform.position + 0.01f * Vector3.up, transform.rotation, transform);
            unitBase.transform.Rotate(90, 0, 0, Space.Self);
            unitBase.transform.localScale = unitRadius * 2 * Vector3.one;
        }
        if(unitBase != null)
        {
            unitBase.SetActive(!IsDead());
        }

        didDamageFlash = false;

        if(ongoingDisplacement != null)
        {


            //Fix This to a better solution involving SelfValidator
            if(photonView.IsMine && (IsAffectedBy(StatusEffectType.Root) || IsAffectedBy(StatusEffectType.Stun) || IsAffectedBy(StatusEffectType.Polymorph) || IsAffectedBy(StatusEffectType.Silence)) && ongoingDisplacement.isFriendly)
            {
                CancelDisplacement();
            }
            else if (ongoingDisplacement.Tick())
            {
                ongoingDisplacement = null;
            }
        }

        if (photonView.IsMine && currentHealth <= 0 && !stat.isDead)
        {
            Kill();
        }

        float walkSpeedMultiplier = stat.finalMovementSpeed / 100f * (100f + statusEffect.status.speed) / 100f * (100f - statusEffect.status.slow) / _defaultMovementSpeed;
        walkSpeedMultiplier = 1 + (walkSpeedMultiplier - 1) * 0.5f;
        _animator.SetFloat("WalkSpeedMultiplier", walkSpeedMultiplier);

        if(_scaleMultipliers.Count == 0)
        {
            _model.localScale = _defaultScale;
        }
        else
        {
            Vector3 scale = _defaultScale;
            for(int i = 0; i < _scaleMultipliers.Count; i++)
            {
                scale *= _scaleMultipliers[i];
            }
            _model.localScale = scale;
        }

        if (IsDead() && shouldDecay)
        {
            _model.localScale = _model.localScale * Mathf.Clamp01(1 - (Time.time - timeOfDeath - startDecayTime) / (endDecayTime - startDecayTime));
            if (Time.time - timeOfDeath > endDecayTime && photonView.IsMine) Destroy();
        }


        if (_flashColors.Count == 0)
        {
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor("_EmissionColor", _defaultEmissionColors[i]);
                _materials[i].SetTexture("_EmissionMap", _defaultEmissionMaps[i]);
                if (_defaultKeywordEnabled[i])
                {
                    _materials[i].EnableKeyword("_EMISSION");
                }
                else
                {
                    _materials[i].DisableKeyword("_EMISSION");
                }
            }
        }
        else
        {
            Color color = Color.clear;
            for(int i = 0; i < _flashColors.Count; i++)
            {
                color += _flashColors[i];
            }

            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor("_EmissionColor", color);
                _materials[i].SetTexture("_EmissionMap", null);
                _materials[i].EnableKeyword("_EMISSION");
            }
        }

        for(int i = _scaleMultipliers.Count - 1; i >= 0; i--)
        {
            _scaleDurations[i] -= Time.deltaTime;
            if(_scaleDurations[i] <= 0)
            {
                _scaleDurations.RemoveAt(i);
                _scaleMultipliers.RemoveAt(i);
            }
        }


        for (int i = _flashColors.Count - 1; i >= 0; i--)
        {
            _flashDurations[i] -= Time.deltaTime;
            if (_flashDurations[i] <= 0)
            {
                _flashColors.RemoveAt(i);
                _flashDurations.RemoveAt(i);
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateCurrentRoom();
        if(GameManager.instance.localPlayer != null)
        {
            switch (GetRelationTo(GameManager.instance.localPlayer))
            {
                case Relation.Own:
                    outline.OutlineColor = UnitControlManager.instance.selfOutlineColor;
                    break;
                case Relation.Ally:
                    outline.OutlineColor = UnitControlManager.instance.allyOutlineColor;
                    break;
                case Relation.Enemy:
                    outline.OutlineColor = UnitControlManager.instance.enemyOutlineColor;
                    break;
            }
            outline.OutlineWidth = 1.5f;
        }

    }



    #endregion Unity

    #region Private Functions

    private void UpdateCustomAnimationNormalizedTime()
    {
        _customAnimationLinearNormalizedTime += Time.deltaTime / _customAnimationDuration;
        if (_customAnimationEaseMethod != null) _animator.SetFloat("CustomAnimationNormalizedTime", _customAnimationEaseMethod(0f, 1f, _customAnimationLinearNormalizedTime));
    }

    private void AssignMissingTransforms()
    {
        if(head == null) head = transform.FindDeepChild("Bip001-Head") ?? transform.FindDeepChild("Bip01 Head");
        if(leftHand == null) leftHand = transform.FindDeepChild("Bip001-L-Hand") ?? transform.FindDeepChild("Bip01 L Hand");
        if(rightHand == null) rightHand =  transform.FindDeepChild("Bip001-R-Hand") ?? transform.FindDeepChild("Bip01 R Hand");
        if(leftFoot == null) leftFoot = transform.FindDeepChild("Bip001-L-Foot") ?? transform.FindDeepChild("Bip01 L Foot");
        if(rightFoot == null) rightFoot = transform.FindDeepChild("Bip001-R-Foot") ?? transform.FindDeepChild("Bip01 R Foot");
        if(top == null) top = transform.FindDeepChild("FXDummy_Head") ?? transform.FindDeepChild("Bip01 Head");
        if(bottom == null) bottom = transform;

        if (head == null) head = transform;
        if (leftHand == null) leftHand = transform;
        if (rightHand == null) rightHand = transform;
        if (leftFoot == null) leftFoot = transform;
        if (rightFoot == null) rightFoot = transform;
        if (top == null) top = transform;

    }

    #endregion

    #region Functions For Everyone

    public bool IsAffectedBy(StatusEffectType type)
    {
        return statusEffect.IsAffectedBy(type);
    }
    public bool HasMana(float amount)
    {
        return stat.currentMana >= amount;
    }
    public bool SpendMana(float amount, DewActionCaller handler)
    {
        if (amount == 0) return true;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to spend mana of negative value! (" + amount.ToString() + ")");
            return true;
        }
        if (stat.currentMana >= amount)
        {
            photonView.RPC(nameof(RpcSpendMana), RpcTarget.All, amount, handler?.GetActionCallerUID());
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool HasGold(float amount)
    {
        return stat.currentGold >= amount;
    }
    public bool IsDead()
    {
        return stat.isDead;
    }
    public bool IsAlive()
    {
        return !stat.isDead;
    }
    public List<Entity> GetAllTargetsInRange(Vector3 center, float range, TargetValidator targetValidator)
    {
        Collider[] colliders = Physics.OverlapSphere(center, range, LayerMask.GetMask("LivingThing"));
        colliders = colliders.OrderBy(collider => Vector3.Distance(center, collider.transform.position)).ToArray();
        List<Entity> result = new List<Entity>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Entity lv = colliders[i].GetComponent<Entity>();
            if (lv != null && !lv.IsDead() && targetValidator.Evaluate(this, lv))
            {
                result.Add(lv);
            }
        }
        return result;
    }
    public List<Entity> GetAllTargetsInLine(Vector3 origin, Vector3 directionVector, float width, float distance, TargetValidator targetValidator)
    {
        RaycastHit[] hits = Physics.SphereCastAll(origin, width / 2f, directionVector, distance, LayerMask.GetMask("LivingThing"));
        hits = hits.OrderBy(hit => Vector3.Distance(origin, hit.collider.transform.position)).ToArray();
        List<Entity> result = new List<Entity>();
        for (int i = 0; i < hits.Length; i++)
        {
            Entity lv = hits[i].collider.GetComponent<Entity>();
            if (lv != null && !lv.IsDead() && targetValidator.Evaluate(this, lv))
            {
                result.Add(lv);
            }
        }
        return result;
    }
    public Entity GetLastAttacker()
    {
        return _lastAttacker ?? this;
    }
    public Relation GetRelationTo(Entity to)
    {
        if (this == to || to.summoner == this) return Relation.Own;
        if (team == Team.None || team != to.team) return Relation.Enemy;
        return Relation.Ally;
    }
    public Vector3 GetCenterOffset()
    {
        Vector3 bottom = this.bottom.position - transform.position;
        Vector3 top = this.top.position - transform.position;

        bottom.x = 0f;
        bottom.z = 0f;
        top.x = 0f;
        top.z = 0f;

        return Vector3.Lerp(bottom, top, 0.5f);
    }
    public Vector3 GetRandomOffset()
    {
        Vector3 bottom = this.bottom.position - transform.position;
        Vector3 top = this.top.position - transform.position;

        return Vector3.Lerp(bottom, top, Random.value);
    }



    public void UpdateCurrentRoom()
    {
        RaycastHit info;
        if (Physics.Raycast(transform.position + 1f * Vector3.up, Vector3.down, out info, 200f, LayerMask.GetMask("Ground")))
        {
            _currentRoom = info.collider.GetComponent<Room>();
            if (_currentRoom == null) _currentRoom = info.collider.transform.parent.GetComponent<Room>();
            if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.GetComponent<Room>();
            if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.parent.GetComponent<Room>();
            if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.parent.parent.GetComponent<Room>();
            // Oh god this is horrifying. Let's fix this later.
        }
    }
    public void CancelDisplacement()
    {
        photonView.RPC(nameof(RpcCancelDisplacement), RpcTarget.All);
    }
    public void SetReadableName(string readableName)
    {
        photonView.RPC(nameof(RpcSetReadableName), RpcTarget.All, readableName);
    }
    public void ApplyStatusEffect(StatusEffect statusEffect, DewActionCaller caller)
    {
        this.statusEffect.ApplyStatusEffect(statusEffect, caller);
    }
    public void Destroy()
    {
        photonView.RPC(nameof(RpcDestroy), RpcTarget.All);
    }
    public void SetCurrentRoom(Room room)
    {
        photonView.RPC(nameof(RpcSetCurrentRoom), RpcTarget.All, room.photonView.ViewID);
    }
    public void ActivateImmediately(Activatable activatable)
    {
        activatable.photonView.RPC("RpcChannelStart", RpcTarget.All, this.photonView.ViewID);
        activatable.photonView.RPC("RpcChannelSuccess", RpcTarget.All, this.photonView.ViewID);
    }

    /// <summary>
    /// Change the walk animation of this entity to a clip with the provided name. Provide an empty string to revert back to default.
    /// </summary>
    /// <param name="animationName"></param>
    public void ChangeWalkAnimation(string animationName = "")
    {
        photonView.RPC(nameof(RpcChangeWalkAnimation), RpcTarget.All, animationName);
    }

    /// <summary>
    /// Change the stand animation of this entity to a clip with the provided name. Provide an empty string to revert back to default.
    /// </summary>
    /// <param name="animationName"></param>
    public void ChangeStandAnimation(string animationName = "")
    {
        photonView.RPC(nameof(RpcChangeStandAnimation), RpcTarget.All, animationName);
    }

    public bool SpendGold(float amount)
    {
        if (amount == 0) return true;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to spend gold of negative value! (" + amount.ToString() + ")");
            return true;
        }
        if (stat.currentGold >= amount)
        {
            photonView.RPC(nameof(RpcSpendGold), RpcTarget.All, amount);
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Teleport(Vector3 location)
    {
        photonView.RPC(nameof(RpcTeleport), RpcTarget.All, location);
    }
    public void LookAt(Vector3 lookPosition, bool immediately = false)
    {
        photonView.RPC(nameof(RpcLookAt), photonView.Owner ?? PhotonNetwork.MasterClient, lookPosition, immediately);
    }
    public void DoHeal(Entity to, float amount, bool ignoreSpellPower, DewActionCaller handler)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do heal of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC(nameof(RpcApplyHeal), RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower, handler?.GetActionCallerUID());
    }
    public void DoManaHeal(Entity to, float amount, bool ignoreSpellPower, DewActionCaller handler)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do mana heal of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC(nameof(RpcApplyManaHeal), RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower, handler?.GetActionCallerUID());
    }
    public void DoBasicAttackImmediately(Entity to, DewActionCaller handler)
    {
        to.photonView.RPC(nameof(RpcApplyBasicAttackDamage), RpcTarget.All, photonView.ViewID, Random.value, handler?.GetActionCallerUID());
    }
    public void DoMagicDamage(Entity to, float amount, bool ignoreSpellPower, DewActionCaller handler)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do magic damage of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC(nameof(RpcApplyMagicDamage), RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower, handler?.GetActionCallerUID());
    }
    public void DoPureDamage(Entity to, float amount, DewActionCaller handler)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do pure damage of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC(nameof(RpcApplyPureDamage), RpcTarget.All, amount, photonView.ViewID, handler?.GetActionCallerUID());
        to.photonView.RPC(nameof(RpcApplyPureDamage), RpcTarget.All, amount, photonView.ViewID, handler?.GetActionCallerUID());
    }
    public void PlayCustomAnimation(AnimationClip animation, float duration = -1, Ease timeCurve = Ease.Linear)
    {
        PlayCustomAnimation(animation.name, duration, timeCurve);
    }
    public void PlayCustomAnimation(string animationName, float duration = -1, Ease timeCurve = Ease.Linear)
    {
        if (duration == 0) return;
        if (duration < 0 && duration != -1)
        {
            Debug.LogWarning(name + ": Attempted to play animation for negative duration! (" + duration.ToString() + ")");
            return;
        }
        photonView.RPC(nameof(RpcPlayCustomAnimation), RpcTarget.All, animationName, duration, (int)timeCurve);
    }
    public void Kill()
    {
        photonView.RPC(nameof(RpcDeath), RpcTarget.All);
    }
    public void Revive()
    {
        photonView.RPC(nameof(RpcRevive), RpcTarget.All);
    }
    public void FlashForDuration(Color color, float multiplier, float duration)
    {
        if (duration == 0) return;
        if (duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to flash for negative duration! (" + duration.ToString() + ")");
            return;
        }
        photonView.RPC(nameof(RpcFlashForDuration), RpcTarget.All, color.r, color.g, color.b, color.a, multiplier, duration);
    }
    public void ScaleForDuration(float multiplier, float duration)
    {
        if (duration == 0) return;
        if (duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to scale for negative duration! (" + duration.ToString() + ")");
            return;
        }
        photonView.RPC(nameof(RpcScaleForDuration), RpcTarget.All, multiplier, duration);
    }
    public void GiveGold(Entity to, float amount)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to give gold of negative amount! (" + amount.ToString() + ")");
            return;
        }
        photonView.RPC(nameof(RpcGiveGold), RpcTarget.All, amount, to.photonView.ViewID);
    }
    public void EarnGold(float amount)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to earn gold of negative amount! (" + amount.ToString() + ")");
            return;
        }
        photonView.RPC(nameof(RpcEarnGold), RpcTarget.All, amount);
    }
    public void StartDisplacement(Displacement displacement)
    {
        if (!displacement.isFriendly && !SelfValidator.CanBePushed.Evaluate(this))
        {
            displacement.SetSelf(this);
            displacement.Cancel();
        }
        else
        {
            if (ongoingDisplacement != null)
            {
                ongoingDisplacement.Cancel();
                ongoingDisplacement = null;
            }

            displacement.SetSelf(this);

            ongoingDisplacement = displacement;

            if (displacement.type == Displacement.DisplacementType.ByVector)
            {
                photonView.RPC(nameof(RpcStartDisplacementByVector), RpcTarget.Others, displacement.isFriendly, displacement.lookForward, displacement.vector, displacement.duration, displacement.ignoreCollision, (byte)displacement.ease);
            }
            else if (displacement.type == Displacement.DisplacementType.TowardsTarget)
            {
                photonView.RPC(nameof(RpcStartDisplacementTowardsTarget), RpcTarget.Others, displacement.isFriendly, displacement.lookForward, displacement.to.photonView.ViewID, displacement.gap, displacement.speed);
            }
        }
    }

    #endregion Functions For Everyone

    #region RPCs
    [PunRPC]
    private void RpcStartDisplacementByVector(bool isFriendly, bool lookForward, Vector3 vector, float duration, bool ignoreCollision, byte ease)
    {
        Displacement displacement = Displacement.ByVector(vector, duration, isFriendly, lookForward, ignoreCollision, (Ease)ease);

        if (ongoingDisplacement != null)
        {
            ongoingDisplacement.Cancel();
            ongoingDisplacement = null;
        }

        ongoingDisplacement = displacement;
    }

    [PunRPC]
    private void RpcStartDisplacementTowardsTarget(bool isFriendly, bool lookForward, int toViewID, float gap, float speed)
    {
        Displacement displacement = Displacement.TowardsTarget(PhotonNetwork.GetPhotonView(toViewID).GetComponent<Entity>(), gap, speed, isFriendly, lookForward);

        if (ongoingDisplacement != null)
        {
            ongoingDisplacement.Cancel();
            ongoingDisplacement = null;
        }

        ongoingDisplacement = displacement;
    }



    [PunRPC]
    private void RpcSetReadableName(string readableName)
    {
        this.readableName = readableName;
    }

    [PunRPC]
    private void RpcDestroy()
    {
        GameManager.instance.OnLivingThingDestroy.Invoke(this);
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        
    }

    [PunRPC]
    private void RpcSetCurrentRoom(int roomViewId)
    {
        Room lastRoom = currentRoom;
        if(lastRoom != null && photonView.IsMine) lastRoom.ToggleTheLights(false);
        currentRoom = PhotonNetwork.GetPhotonView(roomViewId).GetComponent<Room>();
        GameManager.instance.OnLivingThingRoomEnter.Invoke(this);
        if (photonView.IsMine) currentRoom.ActivateRoom(this);
        if (photonView.IsMine) GuidanceArrowManager.RemoveObjective();
    }


    [PunRPC]
    public void RpcFlashForDuration(float r, float g, float b, float a, float multiplier, float duration)
    {
        Color color = new Color(r, g, b, a);
        _flashColors.Add(color * multiplier);
        _flashDurations.Add(duration);
    }

    [PunRPC]
    public void RpcScaleForDuration(float multiplier, float duration)
    {
        _scaleMultipliers.Add(multiplier);
        _scaleDurations.Add(duration);
    }



    [PunRPC]
    protected void RpcRevive()
    {
        stat.isDead = false;
        if(stat.currentHealth == 0)
        {
            stat.currentHealth = 1;
        }
        control.enabled = true;
        control.agent.enabled = true;
        _animator.SetBool("IsDead", false);
    }

    [PunRPC]
    protected void RpcSpendMana(float amount, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);

        stat.currentMana -= amount;
        stat.ValidateMana();
        InfoManaSpent info = new InfoManaSpent { caller = caller, entity = this, amount = amount };
        OnSpendMana.Invoke(info);
        caller?.OnSpendMana.Invoke(info);
    }

    [PunRPC]
    protected void RpcApplyMagicDamage(float amount, int from_id, bool ignoreSpellPower, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) amount = 0f;
        float finalAmount;
        Entity from;

        from = PhotonNetwork.GetPhotonView(from_id).GetComponent<Entity>();
        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        if(statusEffect.status.shield > finalAmount)
        {
            if(photonView.IsMine) statusEffect.ApplyShieldDamage(finalAmount);
        }
        else
        {
            stat.currentHealth -= Mathf.Max(0, finalAmount - statusEffect.status.shield);
            if (photonView.IsMine) statusEffect.ApplyShieldDamage(statusEffect.status.shield);
            stat.ValidateHealth();
        }

        if(from != this) _lastAttacker = from;

        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }

        InfoMagicDamage infoMagicDamage;
        infoMagicDamage.caller = caller;
        infoMagicDamage.to = this;
        infoMagicDamage.from = from;
        infoMagicDamage.originalDamage = amount;
        infoMagicDamage.finalDamage = finalAmount;
        OnTakeMagicDamage.Invoke(infoMagicDamage);
        from.OnDealMagicDamage.Invoke(infoMagicDamage);
        caller?.OnDealMagicDamage.Invoke(infoMagicDamage);

        InfoDamage infoDamage;
        infoDamage.caller = caller;
        infoDamage.damage = amount;
        infoDamage.from = from;
        infoDamage.to = this;
        infoDamage.type = DamageType.Spell;
        OnTakeDamage.Invoke(infoDamage);
        from.OnDealDamage.Invoke(infoDamage);
        caller?.OnDealDamage.Invoke(infoDamage);
    }

    [PunRPC]
    public void RpcLookAt(Vector3 lookPosition, bool immediately)
    {
        control.LookAt(lookPosition, immediately);
    }

    [PunRPC]
    protected void RpcApplyBasicAttackDamage(int from_id, float random, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);
        Entity from = PhotonNetwork.GetPhotonView(from_id).GetComponent<Entity>();

        if (from.statusEffect.IsAffectedBy(StatusEffectType.Blind))
        {
            InfoMiss info;
            info.from = from;
            info.to = this;
            from.OnMiss.Invoke(info);
        }
        else if (random < stat.finalDodgeChance / 100f)
        {
            InfoMiss info;
            info.from = from;
            info.to = this;
            from.OnMiss.Invoke(info);
            OnDodge.Invoke(info);
        }
        else
        {
            float finalAmount;
            finalAmount = from.stat.finalAttackDamage;

            if (!SelfValidator.CanBeDamaged.Evaluate(this)) finalAmount = 0f;

            if (statusEffect.status.shield > finalAmount)
            {
                if (photonView.IsMine) statusEffect.ApplyShieldDamage(finalAmount);
            }
            else
            {
                stat.currentHealth -= Mathf.Max(0, finalAmount - statusEffect.status.shield);
                if (photonView.IsMine) statusEffect.ApplyShieldDamage(statusEffect.status.shield);
                stat.ValidateHealth();
            }

            if(from != this) _lastAttacker = from;

            if (photonView.IsMine)
            {
                stat.SyncChangingStats(); // Is this redundant? check when you're not sleep deprived.
            }

            InfoBasicAttackHit infoBasicAttackHit;
            infoBasicAttackHit.caller = caller;
            infoBasicAttackHit.damage = finalAmount;
            infoBasicAttackHit.from = from;
            infoBasicAttackHit.to = this;
            OnTakeBasicAttackHit.Invoke(infoBasicAttackHit);
            from.OnDoBasicAttackHit.Invoke(infoBasicAttackHit);
            caller?.OnDoBasicAttackHit.Invoke(infoBasicAttackHit);

            InfoDamage infoDamage;
            infoDamage.caller = caller;
            infoDamage.damage = finalAmount;
            infoDamage.from = from;
            infoDamage.to = this;
            infoDamage.type = DamageType.Physical;
            OnTakeDamage.Invoke(infoDamage);
            from.OnDealDamage.Invoke(infoDamage);
            caller?.OnDealDamage.Invoke(infoDamage);
        }
    }

    [PunRPC]
    protected void RpcGiveGold(float amount, int to_id)
    {
        Entity to = PhotonNetwork.GetPhotonView(to_id).GetComponent<Entity>();
        stat.currentGold -= amount;
        if (photonView.IsMine) stat.SyncChangingStats();
        to.stat.currentGold += amount;
        if (to.photonView.IsMine) to.stat.SyncChangingStats();


        InfoGold info;
        info.from = this;
        info.to = to;
        info.amount = amount;
        OnGiveGold.Invoke(info);
        to.OnTakeGold.Invoke(info);
    }


    [PunRPC]
    protected void RpcEarnGold(float amount)
    {
        stat.currentGold += amount;
        if (photonView.IsMine) stat.SyncChangingStats();

        InfoGold info;
        info.to = this;
        info.from = this;
        info.amount = amount;
        OnTakeGold.Invoke(info);
    }




    [PunRPC]
    protected void RpcApplyPureDamage(float amount, int from_id, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) return;
        Entity from = PhotonNetwork.GetPhotonView(from_id).GetComponent<Entity>();
        stat.currentHealth -= Mathf.Max(0, amount);
        stat.ValidateHealth();
        if(from!=this) _lastAttacker = from;
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();   
        }

        InfoDamage info;
        info.caller = caller;
        info.damage = amount;
        info.from = from;
        info.to = this;
        info.type = DamageType.Pure;

        OnTakeDamage.Invoke(info);
        from.OnDealDamage.Invoke(info);
        OnTakePureDamage.Invoke(info);
        from.OnDealPureDamage.Invoke(info);
        caller?.OnDealDamage.Invoke(info);
        caller?.OnDealPureDamage.Invoke(info);
    }



    [PunRPC]
    protected void RpcDeath()
    {
        InfoDeath info;
        Entity killer = GetLastAttacker();

        if (killer == null) killer = this;

        info.killer = killer;
        info.victim = this;
        
        stat.isDead = true;

        stat.currentHealth = 0;
        control.enabled = false;
        control.agent.enabled = false;

        _animator.SetBool("IsDead", true);
        timeOfDeath = Time.time;

        OnDeath.Invoke(info);
        killer.OnKill.Invoke(info);
    }



    [PunRPC]
    protected void RpcApplyHeal(float amount, int from_id, bool ignoreSpellPower, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);
        float finalAmount;
        Entity from = PhotonNetwork.GetPhotonView(from_id).GetComponent<Entity>();

        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        finalAmount = Mathf.Clamp(finalAmount, 0, Mathf.Max(0, stat.finalMaximumHealth - stat.currentHealth));
        
        stat.currentHealth += finalAmount;
        stat.ValidateHealth();
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }

        if (finalAmount <= 0) return;

        InfoHeal info;
        info.caller = caller;
        info.from = from;
        info.to = this;
        info.originalHeal = amount;
        info.finalHeal = finalAmount;
        from.OnDoHeal.Invoke(info);
        OnTakeHeal.Invoke(info);
        caller?.OnDoHeal.Invoke(info);
    }

    [PunRPC]
    protected void RpcApplyManaHeal(float amount, int from_id, bool ignoreSpellPower, int callerUID)
    {
        DewActionCaller caller = DewActionCaller.Retrieve(callerUID);
        float finalAmount;
        Entity from = PhotonNetwork.GetPhotonView(from_id).GetComponent<Entity>();

        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        stat.currentMana += amount;
        stat.ValidateMana();

        InfoManaHeal info;
        info.caller = caller;
        info.from = from;
        info.to = this;
        info.originalManaHeal = amount;
        info.finalManaHeal = finalAmount;
        from.OnDoManaHeal.Invoke(info);
        OnTakeManaHeal.Invoke(info);
        DewActionCaller.Retrieve(callerUID)?.OnDoManaHeal.Invoke(info);
    }

    [PunRPC]
    private void RpcSpendGold(float amount)
    {
        stat.currentGold -= amount;
        if (photonView.IsMine) stat.SyncChangingStats();
        InfoSpendGold info;
        info.entity = this;
        info.amount = amount;
        OnSpendGold.Invoke(info);
    }

    [PunRPC]
    private void RpcChangeWalkAnimation(string name)
    {
        AnimationClip newClip = string.IsNullOrEmpty(name) ? defaultWalk : DewResources.GetAnimationClip(name);
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (AnimationClip oldClip in _defaultClips)
        {
            if (oldClip.name == "Walk")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                _overrideController.ApplyOverrides(overrideList);
            }
        }
    }

    [PunRPC]
    private void RpcChangeStandAnimation(string name)
    {
        AnimationClip newClip = string.IsNullOrEmpty(name) ? defaultStand : DewResources.GetAnimationClip(name);
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (AnimationClip oldClip in _defaultClips)
        {
            if (oldClip.name == "Stand")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                _overrideController.ApplyOverrides(overrideList);
            }
        }
    }


    [PunRPC]
    private void RpcPlayCustomAnimation(string name, float duration, int easeMethod)
    {
        AnimationClip newClip = DewResources.GetAnimationClip(name);
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach(AnimationClip oldClip in _defaultClips)
        {
            if(oldClip.name == "Custom Animation")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                _overrideController.ApplyOverrides(overrideList);
            }
        }
        _animator.SetTrigger("PlayCustomAnimation");
        _customAnimationLinearNormalizedTime = 0f;
        _customAnimationDuration = duration == -1 ? newClip.length : duration;
        _customAnimationEaseMethod = EasingFunction.GetEasingFunction((Ease)easeMethod);
        _animator.SetFloat("CustomAnimationNormalizedTime", 0f);
        _animator.SetFloat("CustomAnimationSpeed", newClip.length / _customAnimationDuration);
    }

    [PunRPC]
    private void RpcTeleport(Vector3 location)
    {
        if(ongoingDisplacement != null && (!ongoingDisplacement.isFriendly || ongoingDisplacement.type == Displacement.DisplacementType.TowardsTarget))
        {
            ongoingDisplacement.Cancel();
            ongoingDisplacement = null;
        }
        control.agent.enabled = false;
        transform.position = location;
        control.agent.enabled = true;
    }

    [PunRPC]
    private void RpcCancelDisplacement()
    {
        if (ongoingDisplacement == null) return;
        ongoingDisplacement.Cancel();
        ongoingDisplacement = null;
    }

    #endregion RPCs
}
