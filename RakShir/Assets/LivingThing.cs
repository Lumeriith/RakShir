using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
[RequireComponent(typeof(PhotonView))]
public abstract class LivingThing : MonoBehaviourPun, IPunObservable
{
    [Header("Health Value")]
    public float maximumHealth;

    [SerializeField]
    public float currentHealth;

    [HideInInspector]
    public LivingThingControl control;




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
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            Dead();
    }

    protected abstract void Dead();

    private void UpdateHealthBar()
    {
    }

    private void UpdateMaxHp(float updateValue)
    {
        maximumHealth = updateValue;
        UpdateHealthBar();
    }
}
