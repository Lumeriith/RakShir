using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public enum Team { None, Red, Blue, Creep }
public enum LivingThingType { Monster, Player, Summon }


public class LivingThing : MonoBehaviourPun
{
    public System.Action OnDeath = () => { };
    public System.Action OnTakeDamage = () => { };
    public System.Action OnDealDamage = () => { };

    public Team team = Team.None;
    public LivingThingType type = LivingThingType.Monster;
    public LivingThing summoner = null;

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

    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if(currentHealth <= 0)
            {
                photonView.RPC("RpcDie", RpcTarget.AllViaServer);
            }
        }
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


    public void DoBasicAttackImmediately(LivingThing to)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(to)) return;
        float finalAmount;
        finalAmount = stat.finalAttackDamage;
        to.photonView.RPC("RpcApplyRawDamage", RpcTarget.AllViaServer, finalAmount);
        photonView.RPC("RpcInvokeOnDealDamage", RpcTarget.AllViaServer);
    }

    public void DoMagicDamage(float amount, LivingThing to)
    {
        if (!SelfValidator.CanBeDamaged.Evaluate(to)) return;
        float finalAmount;
        finalAmount = amount * stat.finalSpellPower / 100;
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
    protected void RpcDie()
    {
        stat.isDead = true;
        OnDeath.Invoke();
    }


}
