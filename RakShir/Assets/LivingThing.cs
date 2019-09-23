using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[RequireComponent(typeof(PhotonView))]
public abstract class LivingThing : MonoBehaviourPun, IPunObservable
{
    [Header("Health Value")]
    public float maxHp;

    [SerializeField]
    protected float currentHp;

    [HideInInspector]
    public LivingThingControl control;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHp);
        }
        else
        {
            maxHp = (float)stream.ReceiveNext();
        }
    }


    private void Awake()
    {
        currentHp = maxHp;
        control = GetComponent<LivingThingControl>();
        
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
            Dead();
    }

    protected abstract void Dead();

    private void UpdateHealthBar()
    {
    }

    private void UpdateMaxHp(float updateValue)
    {
        maxHp = updateValue;
        UpdateHealthBar();
    }
}
