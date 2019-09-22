using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public abstract class LivingThing : MonoBehaviourPun, IPunObservable
{
    [Header("Health Value")]
    public float maxHp;

    [SerializeField]
    protected float currentHp;

    [HideInInspector]
    public LivingThingSpell spell;
    
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
        spell = GetComponent<LivingThingSpell>();
        
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
