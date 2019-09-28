using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using NaughtyAttributes;

#region Enums
public enum Team { None, Red, Blue, Creep }
public enum LivingThingType { Monster, Player, Summon }

public enum Relation { Own, Enemy, Ally }
#endregion Enums

#region Action Info Structs
public struct InfoDeath
{
    public LivingThing victim;
    public LivingThing killer;
}

public struct InfoAbilityCast
{
    public int abilityIndex;
    public CastInfo castInfo;
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

public class LivingThing : MonoBehaviourPun
{
    public GameObject prototypeHealthbarToInstantiate;
    private LivingThing lastAttacker;
    private Animator animator;

    #region Action Declarations

    public System.Action<InfoAbilityCast> OnAbilityCast = (InfoAbilityCast _) => { };

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

    [Header("Location References")]
    public Transform rightHand;
    public Transform head;
    public Transform top;
    public Transform bottom;

    [HideInInspector]
    public LivingThingControl control;
    [HideInInspector]
    public LivingThingStat stat;
    [HideInInspector]
    public LivingThingStatusEffect statusEffect;
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
        control = GetComponent<LivingThingControl>();
        stat = GetComponent<LivingThingStat>();
        statusEffect = GetComponent<LivingThingStatusEffect>();
        gameObject.layer = LayerMask.NameToLayer("LivingThing");

        animator = transform.Find("Model").GetComponent<Animator>();
        
        if (photonView.IsMine)
        {
            GetComponent<NavMeshAgent>().avoidancePriority++;
        }
    }

    private void Start()
    {
        if (prototypeHealthbarToInstantiate != null)
        {
            Instantiate(prototypeHealthbarToInstantiate, Transform.FindObjectOfType<Canvas>().transform, false).GetComponent<PlayerHealthbarPrototype>().targetPlayer = this;
        }
    }

    private void Update()
    {

        if (photonView.IsMine)
        {
            if (currentHealth <= 0 && !stat.isDead)
            {
                Kill();
            }
        }
    }

    #endregion Unity

    #region Functions For Local
    public void CommandMove(Vector3 location)
    {
        if (SelfValidator.CanCommandMove.Evaluate(this))
        {
            control.StartMoving(location);
        }
    }


    public void CommandAttackMove(Vector3 location)
    {
        if (SelfValidator.CanCommandMove.Evaluate(this))
        {
            control.StartAttackMoving(location);
        }
    }
    #endregion Functions For Local

    #region Functions For Everyone

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
        NavMeshPath path = new NavMeshPath();
        Vector3 destination;

        if (NavMesh.CalculatePath(transform.position, location, control.navMeshAgent.areaMask, path))
        {
            destination = path.corners[path.corners.Length - 1];
        }
        else
        {
            destination = location;
            print("Unknown Error!");
        }
        CoreStatusEffect dash = new CoreStatusEffect(this, CoreStatusEffectType.Dash, duration);
        statusEffect.ApplyCoreStatusEffect(dash);
        photonView.RPC("RpcLerp", RpcTarget.All, destination, duration);
    }

    public void DashThroughWithSpeed(Vector3 location, float speed)
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 destination;
        
        if (NavMesh.CalculatePath(transform.position, location, control.navMeshAgent.areaMask, path))
        {
            destination = path.corners[path.corners.Length - 1];
        }
        else
        {
            destination = location;
            print("Unknown Error!");
        }

        float time = Vector3.Distance(transform.position, destination) / (speed/100); // Fix this.

        CoreStatusEffect dash = new CoreStatusEffect(this, CoreStatusEffectType.Dash, time);
        statusEffect.ApplyCoreStatusEffect(dash);
        photonView.RPC("RpcLerp", RpcTarget.AllViaServer, destination, time);
    }

    public void DoHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        to.photonView.RPC("RpcApplyHeal", RpcTarget.AllViaServer, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoBasicAttackImmediately(LivingThing to)
    {
        to.photonView.RPC("RpcApplyBasicAttackDamage", RpcTarget.AllViaServer, photonView.ViewID);
    }

    public void DoMagicDamage(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        to.photonView.RPC("RpcApplyMagicDamage", RpcTarget.AllViaServer, amount, photonView.ViewID, ignoreSpellPower);
    }

    public void DoPureDamage(float amount, LivingThing to)
    {
        to.photonView.RPC("RpcApplyPureDamage", RpcTarget.AllViaServer, amount, photonView.ViewID);
    }

    public void PlaySpellAnimation(AnimationClip animation)
    {
        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        
    }

    public void Kill()
    {
        photonView.RPC("RpcDeath", RpcTarget.AllViaServer);
    }

    #endregion Functions For Everyone

    #region RPCs

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
    }

    [PunRPC]
    protected void RpcApplyBasicAttackDamage(int from_id)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(this)) return;
        
        LivingThing from = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();

        if (from.statusEffect.IsAffectedBy(CoreStatusEffectType.Blind))
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
        }
    }

    [PunRPC]
    protected void RpcApplyPureDamage(float amount, int from_id)
    {
        stat.currentHealth -= Mathf.Max(0, amount);
        stat.ValidateHealth();
        lastAttacker = PhotonNetwork.GetPhotonView(from_id).GetComponent<LivingThing>();
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }
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

        info.killer = killer;
        info.victim = this;
        
        stat.isDead = true;

        OnDeath.Invoke(info);
        killer.OnKill.Invoke(info);
    }

    [PunRPC]
    protected void RpcStartWalking(Vector3 destination)
    {
        InfoStartWalking info;
        info.livingThing = this;
        info.destination = destination;
        OnStartWalking.Invoke(info);
    }

    [PunRPC]
    protected void RpcStopWalking()
    {
        InfoStopWalking info;
        info.livingThing = this;
        OnStopWalking.Invoke(info);
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
    private void RpcLerp(Vector3 destination, float time)
    {
        StartCoroutine(CoroutineLerp(destination, time));
    }


    IEnumerator CoroutineLerp(Vector3 destination, float time)
    {
        float startTime = Time.time;
        Vector3 startPosition = transform.position;

        while(Time.time - startTime < time)
        {
            transform.position = Vector3.Lerp(startPosition, destination, (Time.time - startTime)/time);
            yield return null;
        }
        transform.position = destination;

    }

    #endregion RPCs
}
