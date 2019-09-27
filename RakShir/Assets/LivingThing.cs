using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using NaughtyAttributes;
public enum Team { None, Red, Blue, Creep }
public enum LivingThingType { Monster, Player, Summon }


public class LivingThing : MonoBehaviourPun
{
    public GameObject prototypeHealthbarToInstantiate;

    public System.Action OnDeath = () => { };
    public System.Action OnTakeDamage = () => { };
    public System.Action OnDealDamage = () => { };

    public Team team = Team.None;
    public LivingThingType type = LivingThingType.Monster;

    [ShowIf("ShouldShowSummonerField")]
    public LivingThing summoner = null;

    [Header("Location References")]
    public Transform rightHand;
    public Transform head;
    public Transform top;
    public Transform bottom;

    bool ShouldShowSummonerField()
    {
        return type == LivingThingType.Summon;
    }

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

    [HideInInspector]
    public LivingThingControl control;
    [HideInInspector]
    public LivingThingStat stat;
    [HideInInspector]
    public LivingThingStatusEffect statusEffect;

    private void Awake()
    {
        control = GetComponent<LivingThingControl>();
        stat = GetComponent<LivingThingStat>();
        statusEffect = GetComponent<LivingThingStatusEffect>();
        gameObject.layer = LayerMask.NameToLayer("LivingThing");
        if (photonView.IsMine)
        {
            GetComponent<NavMeshAgent>().avoidancePriority++;
        }
    }

    private void Start()
    {
        if(prototypeHealthbarToInstantiate != null)
        {
            Instantiate(prototypeHealthbarToInstantiate, Transform.FindObjectOfType<Canvas>().transform, false).GetComponent<PlayerHealthbarPrototype>().targetPlayer = this;
        }
    }

    private void Update()
    {
        
        if (photonView.IsMine)
        {
            if(currentHealth <= 0 && !stat.isDead)
            {
                photonView.RPC("RpcDie", RpcTarget.AllViaServer);
            }
        }
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
        photonView.RPC("RpcLerp", RpcTarget.AllViaServer, destination, duration);
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

    public void DoHeal(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        float finalAmount;

        finalAmount = ignoreSpellPower ? amount : amount * stat.finalSpellPower / 100;
        to.photonView.RPC("RpcApplyHeal", RpcTarget.AllViaServer, finalAmount);
        
    }


    public void DoBasicAttackImmediately(LivingThing to)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(to)) return;
        float finalAmount;
        finalAmount = stat.finalAttackDamage;
        if(Random.value < to.stat.baseDodgeChance / 100)
        {
            // Dodged.
        }
        else
        {
            to.photonView.RPC("RpcApplyRawDamage", RpcTarget.AllViaServer, finalAmount);
            photonView.RPC("RpcInvokeOnDealDamage", RpcTarget.AllViaServer);
        }
    }

    public void DoMagicDamage(float amount, LivingThing to, bool ignoreSpellPower = false)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(to)) return;
        float finalAmount;
        
        finalAmount = ignoreSpellPower ? amount : amount * stat.finalSpellPower / 100;
        to.photonView.RPC("RpcApplyRawDamage", RpcTarget.AllViaServer, finalAmount);
        photonView.RPC("RpcInvokeOnDealDamage", RpcTarget.AllViaServer);
    }

    public void DoPureDamage(float amount, LivingThing to)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(to)) return;
        to.photonView.RPC("RpcApplyRawDamage", RpcTarget.AllViaServer, amount);
        photonView.RPC("RpcInvokeOnDealDamage", RpcTarget.AllViaServer);
    }


    [PunRPC]
    protected void RpcInvokeOnDealDamage()
    {
        OnDealDamage.Invoke();
    }


    [PunRPC]
    protected void RpcApplyRawDamage(float amount)
    {
        stat.currentHealth -= Mathf.Max(0, amount);
        stat.ValidateHealth();
        OnTakeDamage.Invoke();
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }
    }


    [PunRPC]
    protected void RpcApplyHeal(float amount)
    {
        stat.currentHealth += amount;
        stat.ValidateHealth();
        if (photonView.IsMine)
        {
            stat.SyncChangingStats();
        }
    }

    [PunRPC]
    protected void RpcDie()
    {
        stat.isDead = true;
        OnDeath.Invoke();
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
        transform.position = destination    ;

    }

}
