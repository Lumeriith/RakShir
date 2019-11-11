using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using NaughtyAttributes;
using System.Linq;

#region Enums
public enum Team { None, Red, Blue, Creep }
public enum LivingThingType { Monster, Player, Summon }

public enum LivingThingTier { None, Lesser, Normal, Elite, Boss }


public enum DamageType { Physical, Spell, Pure }
public enum Relation { Own, Enemy, Ally }



#endregion Enums

#region Action Info Structs
public struct InfoManaSpent
{
    public LivingThing livingThing;
    public float amount;
}

public struct InfoDeath
{
    public LivingThing victim;
    public LivingThing killer;
}

public struct InfoDamage
{
    public LivingThing to;
    public LivingThing from;
    public float damage;
    public DamageType type;
}

public struct InfoMagicDamage
{
    public LivingThing to;
    public LivingThing from;
    public float originalDamage;
    public float finalDamage;
}

public struct InfoHeal
{
    public LivingThing to;
    public LivingThing from;
    public float originalHeal;
    public float finalHeal;
}
public struct InfoManaHeal
{
    public LivingThing to;
    public LivingThing from;
    public float originalManaHeal;
    public float finalManaHeal;
}


public struct InfoBasicAttackHit
{
    public LivingThing to;
    public LivingThing from;
    public float damage;
}

public struct InfoMiss
{
    public LivingThing to;
    public LivingThing from;
}

public struct InfoChannel
{
    public LivingThing livingThing;
    public float remainingTime;
}

public struct InfoStartWalking
{
    public LivingThing livingThing;
    public Vector3 destination;
}

public struct InfoStopWalking
{
    public LivingThing livingThing;
}

public struct InfoGold
{
    public LivingThing from;
    public LivingThing to;
    public float amount;
}

public struct InfoSpendGold
{
    public LivingThing livingThing;
    public float amount;
}


#endregion Action Info Structs

[RequireComponent(typeof(LivingThingControl))]
[RequireComponent(typeof(LivingThingStat))]
[RequireComponent(typeof(LivingThingStatusEffect))]
[RequireComponent(typeof(LivingThingBase))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformViewClassic))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class LivingThing : MonoBehaviourPun
{
    private bool didDamageFlash;

    private NavMeshAgent agent;
    private LivingThing lastAttacker;
    private Animator animator;
    private AnimatorOverrideController aoc;
    private AnimationClip[] defaultClips;

    private List<Color> flashColors = new List<Color>();
    private List<float> flashDurations = new List<float>();

    private Transform model;
    private Vector3 defaultScale;

    private List<float> scaleMultipliers = new List<float>();
    private List<float> scaleDurations = new List<float>();

    private List<Material> materials = new List<Material>();
    private List<Color> defaultEmissionColors = new List<Color>();
    private List<Texture> defaultEmissionMaps = new List<Texture>();
    private List<bool> defaultKeywordEnabled = new List<bool>();

    [HideInInspector]
    public Displacement ongoingDisplacement = null;

    private float defaultMovementSpeed;

    #region Action Declarations
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


    public System.Action<InfoStartWalking> OnStartWalking = (InfoStartWalking _) => { };
    public System.Action<InfoStopWalking> OnStopWalking = (InfoStopWalking _) => { };

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
                RaycastHit info;
                if(Physics.Raycast(transform.position, Vector3.down, out info, 200f, LayerMask.GetMask("Ground")))
                {
                    _currentRoom = info.collider.GetComponent<Room>();
                    if (_currentRoom == null) _currentRoom = info.collider.transform.parent.GetComponent<Room>();
                    if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.GetComponent<Room>();
                    if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.parent.GetComponent<Room>();
                    if (_currentRoom == null) _currentRoom = info.collider.transform.parent.parent.parent.parent.GetComponent<Room>();
                    // Oh god this is horrifying. Let's fix this later.

                }

            }
            return _currentRoom;
        }
        set
        {
            _currentRoom = value;
        }

    }

    private Room _currentRoom;

    [ShowIf("ShouldShowSummonerField")]
    public LivingThing summoner = null;

    [Header("Optional Explicit Transforms")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform top;
    public Transform bottom;

    [HideInInspector]
    public LivingThingControl control;
    [HideInInspector]
    public LivingThingStat stat;
    [HideInInspector]
    public LivingThingStatusEffect statusEffect;
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
        agent = GetComponent<NavMeshAgent>();
        model = transform.Find("Model");

        defaultScale = model.localScale;
        model.localScale = Vector3.zero;

        control = GetComponent<LivingThingControl>();
        stat = GetComponent<LivingThingStat>();
        statusEffect = GetComponent<LivingThingStatusEffect>();
        gameObject.layer = LayerMask.NameToLayer("LivingThing");

        defaultMovementSpeed = stat.baseMovementSpeed;

        animator = transform.Find("Model").GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (photonView.IsMine)
        {
            agent.avoidancePriority++;
        }

        if (type == LivingThingType.Monster && tier == LivingThingTier.Boss)
        {
            agent.avoidancePriority -= 5;
        }
        if (type == LivingThingType.Monster && tier == LivingThingTier.Elite)
        {
            agent.avoidancePriority -= 2;
        }
        if (type == LivingThingType.Player) agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = 0.1f;
        agent.autoRepath = false;
        agent.autoBraking = true;
        defaultClips = animator.runtimeAnimatorController.animationClips;
        aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = aoc;

        Renderer renderer = transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            outline = renderer.gameObject.AddComponent<MeshOutline>();
        }
        else
        {
            outline = transform.Find("Model").gameObject.AddComponent<MeshOutline>();
        }

        outline.enabled = false;

        AssignMissingTransforms();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].material.HasProperty("_EmissionColor") ||
                !renderers[i].material.HasProperty("_EmissionMap")) continue;
            materials.Add(renderers[i].material);
            defaultEmissionColors.Add(materials[materials.Count - 1].GetColor("_EmissionColor"));
            defaultEmissionMaps.Add(materials[materials.Count - 1].GetTexture("_EmissionMap"));
            defaultKeywordEnabled.Add(materials[materials.Count - 1].IsKeywordEnabled("_EMISSION"));
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

        float walkSpeedMultiplier = stat.finalMovementSpeed / 100f * (100f + statusEffect.totalSpeedAmount) / 100f * (100f - statusEffect.totalSlowAmount) / defaultMovementSpeed;
        walkSpeedMultiplier = 1 + (walkSpeedMultiplier - 1) * 0.5f;
        animator.SetFloat("WalkSpeedMultiplier", walkSpeedMultiplier);

        if(scaleMultipliers.Count == 0)
        {
            model.localScale = defaultScale;
        }
        else
        {
            Vector3 scale = defaultScale;
            for(int i = 0; i < scaleMultipliers.Count; i++)
            {
                scale *= scaleMultipliers[i];
            }
            model.localScale = scale;
        }

        if (IsDead() && shouldDecay)
        {
            model.localScale = model.localScale * Mathf.Clamp01(1 - (Time.time - timeOfDeath - startDecayTime) / (endDecayTime - startDecayTime));
            if (Time.time - timeOfDeath > endDecayTime && photonView.IsMine) Destroy();
            return;
        }


        if (flashColors.Count == 0)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor("_EmissionColor", defaultEmissionColors[i]);
                materials[i].SetTexture("_EmissionMap", defaultEmissionMaps[i]);
                if (defaultKeywordEnabled[i])
                {
                    materials[i].EnableKeyword("_EMISSION");
                }
                else
                {
                    materials[i].DisableKeyword("_EMISSION");
                }
            }
        }
        else
        {
            Color color = Color.clear;
            for(int i = 0; i < flashColors.Count; i++)
            {
                color += flashColors[i];
            }

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor("_EmissionColor", color);
                materials[i].SetTexture("_EmissionMap", null);
                materials[i].EnableKeyword("_EMISSION");
            }
        }

        for(int i = scaleMultipliers.Count - 1; i >= 0; i--)
        {
            scaleDurations[i] -= Time.deltaTime;
            if(scaleDurations[i] <= 0)
            {
                scaleDurations.RemoveAt(i);
                scaleMultipliers.RemoveAt(i);
            }
        }


        for (int i = flashColors.Count - 1; i >= 0; i--)
        {
            flashDurations[i] -= Time.deltaTime;
            if (flashDurations[i] <= 0)
            {
                flashColors.RemoveAt(i);
                flashDurations.RemoveAt(i);
            }
        }
    }




    #endregion Unity

    #region Private Functions


    void AssignMissingTransforms()
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

    public void CancelDisplacement()
    {
        photonView.RPC("RpcCancelDisplacement", RpcTarget.All);
    }

    public bool IsAffectedBy(StatusEffectType type)
    {
        return statusEffect.IsAffectedBy(type);
    }

    public void SetReadableName(string readableName)
    {
        photonView.RPC("RpcSetReadableName", RpcTarget.All, readableName);
    }

    public void ApplyStatusEffect(StatusEffect statusEffect)
    {
        this.statusEffect.ApplyStatusEffect(statusEffect);
    }

    public void Destroy()
    {
        photonView.RPC("RpcDestroy", photonView.Owner);
    }

    public void SetCurrentRoom(Room room)
    {
        photonView.RPC("RpcSetCurrentRoom", RpcTarget.All, room.photonView.ViewID);
    }

    public void ActivateImmediately(Activatable activatable)
    {
        activatable.photonView.RPC("RpcChannelStart", RpcTarget.All, this.photonView.ViewID);
        activatable.photonView.RPC("RpcChannelSuccess", RpcTarget.All, this.photonView.ViewID);
    }

    public bool HasMana(float amount)
    {
        return stat.currentMana >= amount;
    }


    public bool SpendMana(float amount)
    {
        if (amount == 0) return true;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to spend mana of negative value! (" + amount.ToString() + ")");
            return true;
        }
        if (stat.currentMana >= amount)
        {
            photonView.RPC("RpcSpendMana", RpcTarget.All, amount);
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


    public bool SpendGold(float amount)
    {
        if (amount == 0) return true;
        if(amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to spend gold of negative value! (" + amount.ToString() + ")");
            return true;
        }
        if (stat.currentGold >= amount)
        {
            photonView.RPC("RpcSpendGold", RpcTarget.All, amount);
            return true;
        }
        else
        {
            return false;
        }
    }




    public bool IsDead()
    {
        return stat.isDead;
    }

    public bool IsAlive()
    {
        return !stat.isDead;
    }

    public List<LivingThing> GetAllTargetsInRange(Vector3 center, float range, TargetValidator targetValidator)
    {
        Collider[] colliders = Physics.OverlapSphere(center, range, LayerMask.GetMask("LivingThing"));
        colliders = colliders.OrderBy(collider => Vector3.Distance(center, collider.transform.position)).ToArray();
        List<LivingThing> result = new List<LivingThing>();
        for(int i =0;i<colliders.Length;i++)
        {
            LivingThing lv = colliders[i].GetComponent<LivingThing>();
            if(lv != null && !lv.IsDead() && targetValidator.Evaluate(this, lv))
            {
                result.Add(lv);
            }
        }
        return result;
    }

    public List<LivingThing> GetAllTargetsInLine(Vector3 origin, Vector3 directionVector, float width, float distance, TargetValidator targetValidator)
    {
        RaycastHit[] hits = Physics.SphereCastAll(origin, width / 2f, directionVector, distance, LayerMask.GetMask("LivingThing"));
        hits = hits.OrderBy(hit => Vector3.Distance(origin, hit.collider.transform.position)).ToArray();
        List<LivingThing> result = new List<LivingThing>();
        for (int i = 0;  i<hits.Length;i++)
        {
            LivingThing lv = hits[i].collider.GetComponent<LivingThing>();
            if (lv != null && !lv.IsDead() && targetValidator.Evaluate(this, lv))
            {
                result.Add(lv);
            }
        }
        return result;
    }

    public void ChangeWalkAnimation(string animationName)
    {
        for (int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if (CustomAnimationBox.instance.animations[i].name == animationName)
            {
                photonView.RPC("RpcChangeWalkAnimation", RpcTarget.All, i);

                return;
            }
        }
        Debug.LogError(name + ": Custom Walk animation '" + animationName + "' must be put in Custom Animation Box before usage!");
    }
    public void ChangeStandAnimation(string animationName)
    {
        for (int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if (CustomAnimationBox.instance.animations[i].name == animationName)
            {
                photonView.RPC("RpcChangeStandAnimation", RpcTarget.All, i);

                return;
            }
        }
        Debug.LogError(name + ": Custom Stand animation '" + animationName + "' must be put in Custom Animation Box before usage!");
    }


    public LivingThing GetLastAttacker()
    {
        return lastAttacker ?? this;
    }

    public Relation GetRelationTo(LivingThing to)
    {
        if (this == to || to.summoner == this) return Relation.Own;
        if (team == Team.None || team != to.team) return Relation.Enemy;
        return Relation.Ally;
    }

    public Vector3 GetCenterOffset()
    {
        Vector3 bottom = this.bottom.position - transform.position;
        Vector3 top = this.top.position - transform.position;

        return Vector3.Lerp(bottom, top, 0.5f);
    }

    public Vector3 GetRandomOffset()
    {
        Vector3 bottom = this.bottom.position - transform.position;
        Vector3 top = this.top.position - transform.position;

        return Vector3.Lerp(bottom, top, Random.value);
    }

    public void Teleport(Vector3 location)
    {
        photonView.RPC("RpcTeleport", RpcTarget.All, location);
    }

    /*
    public void DashThroughForDuration(Vector3 location, float duration)
    {
        if(duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to dash for negative duration! (" + duration.ToString() + ")");
            return;
        }
        CancelDash();
        CancelAirborne();
        NavMeshPath path = new NavMeshPath();
        Vector3 destination;

        if (NavMesh.CalculatePath(transform.position, location, control.agent.areaMask, path))
        {
            destination = path.corners[path.corners.Length - 1];
        }
        else
        {
            destination = location;
        }
        StatusEffect dash = new StatusEffect(this, StatusEffectType.Dash, duration);
        statusEffect.ApplyStatusEffect(dash);
        photonView.RPC("RpcDash", RpcTarget.All, destination, duration);
    }

    public void DashThroughWithSpeed(Vector3 location, float speed)
    {
        if (speed < 0)
        {
            Debug.LogWarning(name + ": Attempted to dash for negative speed! (" + speed.ToString() + ")");
            return;
        }
        CancelDash();
        CancelAirborne();
        NavMeshPath path = new NavMeshPath();
        Vector3 destination;
        
        if (NavMesh.CalculatePath(transform.position, location, control.agent.areaMask, path))
        {
            destination = path.corners[path.corners.Length - 1];
        }
        else
        {
            destination = location;
        }

        float time = Vector3.Distance(transform.position, destination) / (speed); // Fix this.

        StatusEffect dash = new StatusEffect(this, StatusEffectType.Dash, time);
        statusEffect.ApplyStatusEffect(dash);
        photonView.RPC("RpcDash", RpcTarget.All, destination, time);
    }

    public void AirborneForDuration(Vector3 landLocation, float duration)
    {
        if (duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to dash for negative duration! (" + duration.ToString() + ")");
            return;
        }
        if (!SelfValidator.CanBeAirborned.Evaluate(this)) return;

        CancelDash();
        CancelAirborne();
        NavMeshPath path = new NavMeshPath();
        Vector3 destination;

        if (NavMesh.CalculatePath(transform.position, landLocation, control.agent.areaMask, path))
        {
            destination = path.corners[path.corners.Length - 1];
        }
        else
        {
            destination = landLocation;
        }
        StatusEffect airborne = new StatusEffect(this, StatusEffectType.Airborne, duration);
        statusEffect.ApplyStatusEffect(airborne);
        photonView.RPC("RpcAirborne", RpcTarget.All, destination, duration);
    }

    public void CancelAirborne()
    {
        statusEffect.CleanseStatusEffect(StatusEffectType.Airborne);
        photonView.RPC("RpcCancelAirborne", RpcTarget.All);
    }

    public void CancelDash()
    {
        statusEffect.CleanseStatusEffect(StatusEffectType.Dash);
        photonView.RPC("RpcCancelDash", RpcTarget.All);
    }
    */

    public void LookAt(Vector3 lookPosition, bool immediately = false)
    {
        photonView.RPC("RpcLookAt", photonView.Owner ?? PhotonNetwork.MasterClient, lookPosition, immediately);
    }
    public void DoHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do heal of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC("RpcApplyHeal", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoManaHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do mana heal of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC("RpcApplyManaHeal", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoBasicAttackImmediately(LivingThing to)
    {
        to.photonView.RPC("RpcApplyBasicAttackDamage", RpcTarget.All, photonView.ViewID, Random.value);
    }

    public void DoMagicDamage(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do magic damage of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC("RpcApplyMagicDamage", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoPureDamage(float amount, LivingThing to)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to do pure damage of negative amount! (" + amount.ToString() + ")");
            return;
        }
        to.photonView.RPC("RpcApplyPureDamage", RpcTarget.All, amount, photonView.ViewID);
    }

    public void PlayCustomAnimation(AnimationClip animation, float duration = -1)
    {
        if (duration == 0) return;
        if (duration < 0 && duration != -1)
        {
            Debug.LogWarning(name + ": Attempted to play animation for negative duration! (" + duration.ToString() + ")");
            return;
        }
        if (animation == null) return;
        for(int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if(CustomAnimationBox.instance.animations[i] == animation)
            {
                photonView.RPC("RpcPlayCustomAnimation", RpcTarget.All, i, duration);
                
                return;   
            }
        }
        Debug.LogError(name + ": Custom animation '" + animation.name + "' must be put in Custom Animation Box before usage!");
    }

    public void PlayCustomAnimation(string animationName, float duration = -1)
    {
        if (duration == 0) return;
        if (duration < 0 && duration != -1)
        {
            Debug.LogWarning(name + ": Attempted to play animation for negative duration! (" + duration.ToString() + ")");
            return;
        }
        for (int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if (CustomAnimationBox.instance.animations[i].name == animationName)
            {
                photonView.RPC("RpcPlayCustomAnimation", RpcTarget.All, i, duration);
                
                return;
            }
        }
        Debug.LogError(name + ": Custom animation '" + animationName + "' must be put in Custom Animation Box before usage!");
    }

    public void Kill()
    {
        photonView.RPC("RpcDeath", RpcTarget.All);
    }

    public void Revive()
    {
        photonView.RPC("RpcRevive", RpcTarget.All);
    }

    public void FlashForDuration(Color color, float multiplier, float duration)
    {
        if (duration == 0) return;
        if (duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to flash for negative duration! (" + duration.ToString() + ")");
            return;
        }
        photonView.RPC("RpcFlashForDuration", RpcTarget.All, color.r, color.g, color.b, color.a, multiplier, duration);
    }

    public void ScaleForDuration(float multiplier, float duration)
    {
        if (duration == 0) return;
        if (duration < 0)
        {
            Debug.LogWarning(name + ": Attempted to scale for negative duration! (" + duration.ToString() + ")");
            return;
        }
        photonView.RPC("RpcScaleForDuration", RpcTarget.All, multiplier, duration);
    }



    public void GiveGold(float amount, LivingThing to)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to give gold of negative amount! (" + amount.ToString() + ")");
            return;
        }
        photonView.RPC("RpcGiveGold", RpcTarget.All, amount, to.photonView.ViewID);
    }

    public void EarnGold(float amount)
    {
        if (amount == 0) return;
        if (amount < 0)
        {
            Debug.LogWarning(name + ": Attempted to earn gold of negative amount! (" + amount.ToString() + ")");
            return;
        }
        photonView.RPC("RpcEarnGold", RpcTarget.All, amount);
    }
    public void StartDisplacement(Displacement displacement)
    {
        if(!displacement.isFriendly && !SelfValidator.CanBePushed.Evaluate(this))
        {
            if (!displacement.isTargetDisplacement) displacement.SetStartPosition(transform.position);

            
            displacement.self = this;
            displacement.Cancel();

        }
        else
        {
            if (ongoingDisplacement != null)
            {
                ongoingDisplacement.Cancel();
                ongoingDisplacement = null;
            }

            if (!displacement.isTargetDisplacement) displacement.SetStartPosition(transform.position);

            ongoingDisplacement = displacement;
            ongoingDisplacement.self = this;

            photonView.RPC("RpcStartDisplacement", RpcTarget.Others,
        displacement.isTargetDisplacement,
        displacement.vector1,
        displacement.vector2,
        displacement.duration,
        displacement.isFriendly,
        displacement.lookForward,
        (int)displacement.ease);
        }


    }

    #endregion Functions For Everyone

    #region RPCs
    [PunRPC]
    private void RpcStartDisplacement(bool isTargetDisplacement, Vector3 vector1, Vector3 vector2, float duration, bool isFriendly, bool lookForward, int ease)
    {
        Displacement displacement = new Displacement
        {
            isTargetDisplacement = isTargetDisplacement,
            vector1 = vector1,
            vector2 = vector2,
            duration = duration,
            isFriendly = isFriendly,
            lookForward = lookForward,
            ease = (EasingFunction.Ease)ease
        };

        if (ongoingDisplacement != null)
        {
            ongoingDisplacement.Cancel();
            ongoingDisplacement = null;
        }


        ongoingDisplacement = displacement;
        ongoingDisplacement.self = this;
    }


    [PunRPC]
    private void RpcSetReadableName(string readableName)
    {
        this.readableName = readableName;
    }

    [PunRPC]
    private void RpcDestroy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RpcSetCurrentRoom(int roomViewId)
    {
        currentRoom = PhotonNetwork.GetPhotonView(roomViewId).GetComponent<Room>();
        GameManager.instance.OnLivingThingRoomEnter.Invoke(this);
        if (photonView.IsMine && !currentRoom.isActivated) currentRoom.ActivateRoom(this);
    }


    [PunRPC]
    public void RpcFlashForDuration(float r, float g, float b, float a, float multiplier, float duration)
    {
        Color color = new Color(r, g, b, a);
        flashColors.Add(color * multiplier);
        flashDurations.Add(duration);
    }

    [PunRPC]
    public void RpcScaleForDuration(float multiplier, float duration)
    {
        scaleMultipliers.Add(multiplier);
        scaleDurations.Add(duration);
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
        animator.SetBool("IsDead", false);
    }

    [PunRPC]
    protected void RpcSpendMana(float amount)
    {
        stat.currentMana -= amount;
        stat.ValidateMana();

        InfoManaSpent info;
        info.livingThing = this;
        info.amount = amount;
        OnSpendMana.Invoke(info);
    }

    [PunRPC]
    protected void RpcApplyMagicDamage(float amount, int from_id, bool ignoreSpellPower)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) return;
        float finalAmount;
        LivingThing from;

        from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();
        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        if(statusEffect.totalShieldAmount > finalAmount)
        {
            if(photonView.IsMine) statusEffect.ApplyShieldDamage(finalAmount);
        }
        else
        {
            stat.currentHealth -= Mathf.Max(0, finalAmount - statusEffect.totalShieldAmount);
            if (photonView.IsMine) statusEffect.ApplyShieldDamage(statusEffect.totalShieldAmount);
            stat.ValidateHealth();
        }

        if(from != this) lastAttacker = from;

        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }

        InfoMagicDamage info;
        info.to = this;
        info.from = from;
        info.originalDamage = amount;
        info.finalDamage = finalAmount;
        OnTakeMagicDamage.Invoke(info);
        info.from.OnDealMagicDamage.Invoke(info);

        InfoDamage info2;
        info2.damage = amount;
        info2.from = from;
        info2.to = this;
        info2.type = DamageType.Spell;
        OnTakeDamage.Invoke(info2);
        from.OnDealDamage.Invoke(info2);
    }

    [PunRPC]
    public void RpcLookAt(Vector3 lookPosition, bool immediately)
    {
        control.LookAt(lookPosition, immediately);
    }

    [PunRPC]
    protected void RpcApplyBasicAttackDamage(int from_id, float random)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) return;
        
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

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

            if (statusEffect.totalShieldAmount > finalAmount)
            {
                if (photonView.IsMine) statusEffect.ApplyShieldDamage(finalAmount);
            }
            else
            {
                stat.currentHealth -= Mathf.Max(0, finalAmount - statusEffect.totalShieldAmount);
                if (photonView.IsMine) statusEffect.ApplyShieldDamage(statusEffect.totalShieldAmount);
                stat.ValidateHealth();
            }

            if(from != this) lastAttacker = from;

            if (photonView.IsMine)
            {
                stat.SyncChangingStats(); // Is this redundant? check when you're not sleep deprived.
            }

            InfoBasicAttackHit info;
            info.damage = finalAmount;
            info.from = from;
            info.to = this;
            OnTakeBasicAttackHit.Invoke(info);
            from.OnDoBasicAttackHit.Invoke(info);

            InfoDamage info2;
            info2.damage = finalAmount;
            info2.from = from;
            info2.to = this;
            info2.type = DamageType.Physical;
            OnTakeDamage.Invoke(info2);
            from.OnDealDamage.Invoke(info2);
        }
    }

    [PunRPC]
    protected void RpcGiveGold(float amount, int to_id)
    {
        LivingThing to = PhotonNetwork.GetPhotonView(to_id).GetComponent<LivingThing>();
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
    protected void RpcApplyPureDamage(float amount, int from_id)
    {
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();
        stat.currentHealth -= Mathf.Max(0, amount);
        stat.ValidateHealth();
        if(from!=this) lastAttacker = from;
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }

        InfoDamage info2;
        info2.damage = amount;
        info2.from = from;
        info2.to = this;
        info2.type = DamageType.Pure;
        OnTakeDamage.Invoke(info2);
        from.OnDealDamage.Invoke(info2);
        OnTakePureDamage.Invoke(info2);
        from.OnDealPureDamage.Invoke(info2);
    }



    [PunRPC]
    protected void RpcDeath()
    {
        InfoDeath info;
        LivingThing killer = GetLastAttacker();

        if (killer == null) killer = this;

        info.killer = killer;
        info.victim = this;
        
        stat.isDead = true;

        stat.currentHealth = 0;
        control.enabled = false;
        control.agent.enabled = false;
        OnDeath.Invoke(info);
        
        killer.OnKill.Invoke(info);
        animator.SetBool("IsDead", true);
        timeOfDeath = Time.time;
    }



    [PunRPC]
    protected void RpcApplyHeal(float amount, int from_id, bool ignoreSpellPower)
    {
        float finalAmount;
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

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
        info.from = from;
        info.to = this;
        info.originalHeal = amount;
        info.finalHeal = finalAmount;
        from.OnDoHeal.Invoke(info);
        OnTakeHeal.Invoke(info);
    }

    [PunRPC]
    protected void RpcApplyManaHeal(float amount, int from_id, bool ignoreSpellPower)
    {
        float finalAmount;
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        stat.currentMana += amount;
        stat.ValidateMana();

        InfoManaHeal info;
        info.from = from;
        info.to = this;
        info.originalManaHeal = amount;
        info.finalManaHeal = finalAmount;
        from.OnDoManaHeal.Invoke(info);
        OnTakeManaHeal.Invoke(info);
    }




    private Coroutine lastDashCoroutine;
    private Coroutine lastAirborneCoroutine;

    [PunRPC]
    private void RpcDash(Vector3 destination, float time)
    {
        lastDashCoroutine = StartCoroutine(CoroutineDash(destination, time));
    }
    [PunRPC]
    private void RpcAirborne(Vector3 destination, float time)
    {
        lastAirborneCoroutine = StartCoroutine(CoroutineAirborne(destination, time));
    }


    [PunRPC]
    private void RpcSpendGold(float amount)
    {
        stat.currentGold -= amount;
        if (photonView.IsMine) stat.SyncChangingStats();
        InfoSpendGold info;
        info.livingThing = this;
        info.amount = amount;
        OnSpendGold.Invoke(info);
    }

    [PunRPC]
    private void RpcChangeWalkAnimation(int index)
    {
        AnimationClip newClip = CustomAnimationBox.instance.animations[index];
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (AnimationClip oldClip in defaultClips)
        {
            if (oldClip.name == "Walk")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                aoc.ApplyOverrides(overrideList);
            }
        }
        animator.runtimeAnimatorController = aoc;
    }

    [PunRPC]
    private void RpcChangeStandAnimation(int index)
    {
        AnimationClip newClip = CustomAnimationBox.instance.animations[index];
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (AnimationClip oldClip in defaultClips)
        {
            if (oldClip.name == "Stand")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                aoc.ApplyOverrides(overrideList);
            }
        }
        animator.runtimeAnimatorController = aoc;
    }


    [PunRPC]
    private void RpcPlayCustomAnimation(int index, float duration)
    {
        AnimationClip newClip = CustomAnimationBox.instance.animations[index];
        var overrideList = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach(AnimationClip oldClip in defaultClips)
        {
            if(oldClip.name == "Custom Animation")
            {
                overrideList.Add(new KeyValuePair<AnimationClip, AnimationClip>(oldClip, newClip));
                aoc.ApplyOverrides(overrideList);
            }
        }
        animator.runtimeAnimatorController = aoc;
        animator.SetTrigger("PlayCustomAnimation");
        animator.SetFloat("CustomAnimationSpeed", duration == -1 ? 1f : newClip.length / duration);
    }

    [PunRPC]
    private void RpcTeleport(Vector3 location)
    {
        if(ongoingDisplacement != null && (!ongoingDisplacement.isFriendly || ongoingDisplacement.isTargetDisplacement))
        {
            ongoingDisplacement.Cancel();
            ongoingDisplacement = null;
        }
        control.agent.enabled = false;
        transform.position = location;
        control.agent.enabled = true;
    }
    /*
    [PunRPC]
    private void RpcCancelDash()
    {
        if (lastDashCoroutine == null) return;
        StopCoroutine(lastDashCoroutine);
        
    }

    [PunRPC]
    private void RpcCancelAirborne()
    {
        if (lastAirborneCoroutine == null) return;
        StopCoroutine(lastAirborneCoroutine);
    }
    */

    [PunRPC]
    private void RpcCancelDisplacement()
    {
        if (ongoingDisplacement == null) return;
        ongoingDisplacement.Cancel();
        ongoingDisplacement = null;
    }
    



    #endregion RPCs


    #region Coroutines
    IEnumerator CoroutineAirborne(Vector3 destination, float time)
    {
        float startTime = Time.time;
        Vector3 startPosition = transform.position;

        while (Time.time - startTime < time)
        {
            transform.position = Vector3.Lerp(startPosition, destination, (Time.time - startTime) / time);
            yield return null;
        }
        transform.position = destination;
        lastAirborneCoroutine = null;
    }


    IEnumerator CoroutineDash(Vector3 destination, float time)
    {
        float startTime = Time.time;
        Vector3 startPosition = transform.position;

        while (Time.time - startTime < time)
        {
            transform.position = Vector3.Lerp(startPosition, destination, (Time.time - startTime) / time);
            yield return null;
        }
        transform.position = destination;
        lastDashCoroutine = null;
    }

    #endregion Coroutines
}
