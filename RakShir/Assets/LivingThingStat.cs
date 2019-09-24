using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public class LivingThingStat : MonoBehaviourPun, IPunObservable
{
    [Header("Base Stats")]
    public int strength;
    public int wisdom;
    public int dexterity;

    [Header("Bonus Stats")]
    public float bonusHealth;
    public float bonusAttackDamage;
    public float bonusCooldownReduction;
    public float bonusSpellPowerMultiplier;
    public float bonusMovementSpeedMultiplier;
    public float bonusAttackSpeedMultiplier;

    public float bonusPhysicalArmor;
    public float bonusSpellArmor;
    
    [BoxGroup("Scaling Per Strength")]
    [SerializeField]
    private float healthPerUnit;

    [BoxGroup("Scaling Per Strength")]
    [SerializeField]
    private float attackDamagePerUnit;

    [BoxGroup("Scaling Per Wisdom")]
    [SerializeField]
    private float cooldownReductionPerUnit;

    [BoxGroup("Scaling Per Wisdom")]
    [SerializeField]
    private float spellPowerMultiplierPerUnit;

    [BoxGroup("Scaling Per Dexterity")]
    [SerializeField]
    private float movementSpeedMultiplierPerUnit;

    [BoxGroup("Scaling Per Dexterity")]
    [SerializeField]
    private float attackSpeedMultiplierPerUnit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(0f, 0.95f)]
    private Vector2 finalCooldownReductionLimit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(0f, 10f)]
    private Vector2 finalSpellPowerMultiplierLimit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(0.05f, 10f)]
    private Vector2 finalMovementSpeedMultiplierLimit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(0.05f, 10f)]
    private Vector2 finalAttackSpeedMultiplierLimit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(-5f, 1f)]
    private Vector2 finalPhysicalArmorLimit;

    [BoxGroup("Limit Settings")]
    [SerializeField]
    [MinMaxSlider(-5f, 1f)]
    private Vector2 finalSpellArmorLimit;



    public float finalHealthGranted
    {
        get
        {
            return strength * healthPerUnit + bonusHealth;
        }
    }

    public float finalAttackDamageGranted
    {
        get
        {
            return strength * attackDamagePerUnit + bonusAttackDamage;
        }
    }

    public float finalCooldownReductionGranted
    {
        get
        {
            return wisdom * cooldownReductionPerUnit + bonusCooldownReduction;
        }
    }

    public float finalSpellPowerMultiplier
    {
        get
        {
            return 1 + wisdom * spellPowerMultiplierPerUnit + bonusSpellPowerMultiplier;
        }
    }

    public float finalMovementSpeedMultiplier
    {
        get
        {
            return 1 + dexterity * movementSpeedMultiplierPerUnit + bonusMovementSpeedMultiplier;
        }
    }

    public float finalAttackSpeedMultiplier
    {
        get
        {
            return 1 + dexterity * attackSpeedMultiplierPerUnit + bonusAttackSpeedMultiplier;
        }
    }

    public float finalPhysicalArmor
    {
        get
        {
            return bonusPhysicalArmor;
        }
    }

    public float finalSpellArmor
    {
        get
        {
            return bonusSpellArmor;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(strength);
            stream.SendNext(wisdom);
            stream.SendNext(dexterity);
            stream.SendNext(bonusHealth);
            stream.SendNext(bonusAttackDamage);
            stream.SendNext(bonusCooldownReduction);
            stream.SendNext(bonusSpellPowerMultiplier);
            stream.SendNext(bonusMovementSpeedMultiplier);
            stream.SendNext(bonusAttackSpeedMultiplier);
            stream.SendNext(bonusPhysicalArmor);
            stream.SendNext(bonusSpellArmor);
        }
        else
        {
            strength = (int)stream.ReceiveNext();
            wisdom = (int)stream.ReceiveNext();
            dexterity = (int)stream.ReceiveNext();
            bonusHealth = (float)stream.ReceiveNext();
            bonusAttackDamage = (float)stream.ReceiveNext();
            bonusCooldownReduction = (float)stream.ReceiveNext();
            bonusSpellPowerMultiplier = (float)stream.ReceiveNext();
            bonusMovementSpeedMultiplier = (float)stream.ReceiveNext();
            bonusAttackSpeedMultiplier = (float)stream.ReceiveNext();
            bonusPhysicalArmor = (float)stream.ReceiveNext();
            bonusSpellArmor = (float)stream.ReceiveNext();
        }
    }


}
