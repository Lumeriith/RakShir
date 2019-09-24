using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[RequireComponent(typeof(PhotonView))]
public abstract class LivingThing : MonoBehaviourPun, IPunObservable
{
    public float baseMaximumHealth;

    [SerializeField]
    public float currentHealth;

    [HideInInspector]
    public LivingThingControl control;
    [HideInInspector]
    public LivingThingStat stat;

    public float maximumHealth
    {
        get
        {
            if(stat != null)
            {
                return baseMaximumHealth + stat.finalHealthGranted;
            }
            return baseMaximumHealth;
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHealth);
        }
        else
        {
            currentHealth = (float)stream.ReceiveNext();
        }
    }
    private void Awake()
    {
        control = GetComponent<LivingThingControl>();
        stat = GetComponent<LivingThingStat>();
    }

    public void ApplyMagicDamage(float amount, LivingThing attacker = null)
    {
        float finalAmount = amount;

        if (attacker.stat != null)
        {
            finalAmount = finalAmount * attacker.stat.finalSpellPowerMultiplier;
        }

        if(stat != null)
        {
            finalAmount = finalAmount * (1f - stat.finalSpellArmor);
        }

        photonView.RPC("RpcApplyDamage", RpcTarget.AllViaServer, finalAmount);
    }

    public void ApplyNormalDamage(float amount, LivingThing attacker = null)
    {
        float finalAmount = amount;

        if (attacker.stat != null)
        {
            finalAmount = finalAmount + attacker.stat.finalAttackDamageGranted;
        }

        if(stat != null)
        {
            finalAmount = finalAmount * (1f - stat.finalPhysicalArmor);
        }

        photonView.RPC("RpcApplyDamage", RpcTarget.AllViaServer, finalAmount);
    }

    public void ApplyPureDamage(float amount, LivingThing attacker = null)
    {
        photonView.RPC("RpcApplyDamage", RpcTarget.AllViaServer, amount);
    }

    [PunRPC]
    protected void RpcApplyDamage(float amount)
    {
        currentHealth -= Mathf.Max(0, amount);
    }





}
