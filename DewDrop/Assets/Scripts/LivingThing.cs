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
    private LivingThing lastAttacker;
    private Animator animator;
    private AnimatorOverrideController aoc;
    private AnimationClip[] defaultClips;

    #region Action Declarations
    public System.Action<InfoDamage> OnDealDamage = (InfoDamage _) => { };
    public System.Action<InfoDamage> OnTakeDamage = (InfoDamage _) => { };

    public System.Action<InfoMagicDamage> OnDealMagicDamage = (InfoMagicDamage _) => { };
    public System.Action<InfoMagicDamage> OnTakeMagicDamage = (InfoMagicDamage _) => { };

    public System.Action<InfoChannel> OnChannelBasicAttack = (InfoChannel _) => { };
    public System.Action<InfoChannel> OnChannelBasicAttackCanceled = (InfoChannel _) => { };
    public System.Action<InfoChannel> OnChannelBasicAttackSuccess = (InfoChannel _) => { };



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
    public Team team = Team.None;
    public LivingThingType type = LivingThingType.Monster;

    [ShowIf("ShouldShowSummonerField")]
    public LivingThing summoner = null;
    [Header("Infobar Settings")]
    public GameObject infobar;
    [Header("Location References")]
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
    public Outline outline;

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

    #endregion References For Everyone

    #region Unity
    private void Awake()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        control = GetComponent<LivingThingControl>();
        stat = GetComponent<LivingThingStat>();
        statusEffect = GetComponent<LivingThingStatusEffect>();
        gameObject.layer = LayerMask.NameToLayer("LivingThing");

        animator = transform.Find("Model").GetComponent<Animator>();
        
        if (photonView.IsMine)
        {
            GetComponent<NavMeshAgent>().avoidancePriority++;
        }
        defaultClips = animator.runtimeAnimatorController.animationClips;
        aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = aoc;

        if(infobar != null)
        {
            Instantiate(infobar, Vector3.zero, Quaternion.identity, transform.Find("/Infobar Canvas")).GetComponent<IInfobar>().SetTarget(this);
        }

        outline = transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>().gameObject.AddComponent<Outline>();
        outline.enabled = false;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (currentHealth <= 0 && !stat.isDead)
        {
            Kill();
        }

    }

    #endregion Unity



    #region Functions For Everyone



    public bool HasMana(float amount)
    {
        return stat.currentMana > amount;
    }

    public bool SpendMana(float amount)
    {
        if(stat.currentMana > amount)
        {
            photonView.RPC("RpcSpendMana", RpcTarget.All, amount);
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
        Collider[] colliders = Physics.OverlapSphere(center, 4f, LayerMask.GetMask("LivingThing"));
        colliders = colliders.OrderBy(collider => Vector3.Distance(center, collider.transform.position)).ToArray();
        List<LivingThing> result = new List<LivingThing>();
        foreach(Collider collider in colliders)
        {
            LivingThing lv = collider.GetComponent<LivingThing>();
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
        foreach (RaycastHit hit in hits)
        {
            LivingThing lv = hit.collider.GetComponent<LivingThing>();
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
        Debug.LogError("Custom Walk animation '" + animationName + "' must be put in Custom Animation Box before usage!");
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
        Debug.LogError("Custom Stand animation '" + animationName + "' must be put in Custom Animation Box before usage!");
    }


    public LivingThing GetLastAttacker()
    {
        return lastAttacker;
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


    public void DashThroughForDuration(Vector3 location, float duration)
    {
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

    public void LookAt(Vector3 lookPosition, bool immediately = false)
    {
        photonView.RPC("RpcLookAt",photonView.Owner, lookPosition, immediately);
    }
    public void DoHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        to.photonView.RPC("RpcApplyHeal", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoManaHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        to.photonView.RPC("RpcApplyManaHeal", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoBasicAttackImmediately(LivingThing to)
    {
        to.photonView.RPC("RpcApplyBasicAttackDamage", RpcTarget.All, photonView.ViewID);
    }

    public void DoMagicDamage(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        to.photonView.RPC("RpcApplyMagicDamage", RpcTarget.All, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoPureDamage(float amount, LivingThing to)
    {
        to.photonView.RPC("RpcApplyPureDamage", RpcTarget.All, amount, photonView.ViewID);
    }

    public void PlayCustomAnimation(AnimationClip animation, float duration = -1)
    {
        for(int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if(CustomAnimationBox.instance.animations[i] == animation)
            {
                photonView.RPC("RpcPlayCustomAnimation", RpcTarget.All, i, duration);
                
                return;   
            }
        }
        Debug.LogError("Custom animation '" + animation.name + "' must be put in Custom Animation Box before usage!");
    }

    public void PlayCustomAnimation(string animationName, float duration = -1)
    {
        for (int i = 0; i < CustomAnimationBox.instance.animations.Count; i++)
        {
            if (CustomAnimationBox.instance.animations[i].name == animationName)
            {
                photonView.RPC("RpcPlayCustomAnimation", RpcTarget.All, i, duration);
                
                return;
            }
        }
        Debug.LogError("Custom animation '" + animationName + "' must be put in Custom Animation Box before usage!");
    }

    public void Kill()
    {
        photonView.RPC("RpcDeath", RpcTarget.All);
    }

    public void Revive()
    {
        photonView.RPC("RpcRevive", RpcTarget.All);
    }

    #endregion Functions For Everyone

    #region RPCs

    [PunRPC]
    protected void RpcRevive()
    {
        stat.isDead = false;
        if(stat.currentHealth == 0)
        {
            stat.currentHealth = 1;
        }
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


        stat.currentHealth -= Mathf.Max(0, finalAmount);
        stat.ValidateHealth();
        lastAttacker = from;
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
    protected void RpcLookAt(Vector3 lookPosition, bool immediately)
    {
        control.LookAt(lookPosition, immediately);
    }

    [PunRPC]
    protected void RpcApplyBasicAttackDamage(int from_id)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) return;
        
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

        if (from.statusEffect.IsAffectedBy(StatusEffectType.Blind))
        {
            InfoMiss info;
            info.from = from;
            info.to = this;
            OnMiss.Invoke(info);
        }
        else if (Random.value < stat.baseDodgeChance / 100)
        {
            InfoMiss info;
            info.from = from;
            info.to = this;
            OnMiss.Invoke(info);
            OnDodge.Invoke(info);
        }
        else
        {
            float finalAmount;
            finalAmount = from.stat.finalAttackDamage;
            stat.currentHealth -= Mathf.Max(0, finalAmount);
            stat.ValidateHealth();
            lastAttacker = from;
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
    protected void RpcApplyPureDamage(float amount, int from_id)
    {
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();
        stat.currentHealth -= Mathf.Max(0, amount);
        stat.ValidateHealth();
        lastAttacker = from;
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
    }

    [PunRPC]
    protected void RpcChannelBasicAttack(float remainingTime)
    {
        InfoChannel info;
        info.remainingTime = remainingTime;
        info.livingThing = this;
        OnChannelBasicAttack.Invoke(info);
    }

    [PunRPC]
    protected void RpcChannelBasicAttackCanceled(float remainingTime)
    {
        InfoChannel info;
        info.remainingTime = remainingTime;
        info.livingThing = this;
        OnChannelBasicAttackCanceled.Invoke(info);
    }

    [PunRPC]
    protected void RpcChannelBasicAttackSuccess()
    {
        InfoChannel info;
        info.remainingTime = 0;
        info.livingThing = this;
        OnChannelBasicAttackSuccess.Invoke(info);
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

        OnDeath.Invoke(info);
        
        killer.OnKill.Invoke(info);
    }



    [PunRPC]
    protected void RpcApplyHeal(float amount, int from_id, bool ignoreSpellPower)
    {
        float finalAmount;
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

        finalAmount = ignoreSpellPower ? amount : amount * from.stat.finalSpellPower / 100;

        stat.currentHealth += amount;
        stat.ValidateHealth();
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }

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
