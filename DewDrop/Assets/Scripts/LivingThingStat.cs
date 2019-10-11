using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class LivingThingStat : MonoBehaviourPun, IOnEventCallback
{

    private LivingThing livingThing;

    [Header("Changing Stats")]
    public float currentHealth = 1000;
    public float currentMana;
    public bool isDead;

    [Header("Base Stats")]
    public float baseMaximumHealth = 1000;
    public float baseHealthRegenerationPerSecond;

    public float baseMaximumMana;
    public float baseManaRegenerationPerSecond;

    public float baseMovementSpeed = 300;

    public float baseAttackDamage = 20;
    public float baseAttacksPerSecond = 1;

    public float baseSpellPower = 100;

    public float baseCooldownReduction;

    public float baseDodgeChance;

    [Header("Secondary Stats")]
    public float strength;
    public float agility;
    public float intelligence;

    [SerializeField]
    private bool showTemporaryAttributes;
    [Header("Temporary Attributes")]
    [ShowIf("showTemporaryAttributes")]
    public float bonusMaximumHealth;
    [ShowIf("showTemporaryAttributes")]
    public float bonusHealthRegenerationPerSecond;

    [ShowIf("showTemporaryAttributes")]
    public float bonusMaximumMana;
    [ShowIf("showTemporaryAttributes")]
    public float bonusManaRegenerationPerSecond;

    [ShowIf("showTemporaryAttributes")]
    public float bonusMovementSpeed;

    [ShowIf("showTemporaryAttributes")]
    public float bonusAttackDamage;
    [ShowIf("showTemporaryAttributes")]
    public float bonusAttackSpeedPercentage;

    [ShowIf("showTemporaryAttributes")]
    public float bonusSpellPower;

    [ShowIf("showTemporaryAttributes")]
    public float bonusCooldownReduction;

    [ShowIf("showTemporaryAttributes")]
    public float bonusDodgeChance;

    [ShowIf("showTemporaryAttributes")]
    public float bonusStrength;
    [ShowIf("showTemporaryAttributes")]
    public float bonusAgility;
    [ShowIf("showTemporaryAttributes")]
    public float bonusIntelligence;

    [SerializeField]
    private bool showScalingSettings;

    [Header("Scaling Per Strength")]
    [ShowIf("showScalingSettings")]
    public float additionalMaximumHealthPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalHealthRegenerationPerSecondPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalAttackDamagePerUnit;
    [Header("Scaling Per Agility")]
    [ShowIf("showScalingSettings")]
    public float additionalMovementSpeedPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalAttackSpeedPercentagePerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalDodgeChancePerUnit;
    [Header("Scaling Per Intelligence")]
    [ShowIf("showScalingSettings")]
    public float additionalMaximumManaPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalManaRegenerationPerSecondPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalSpellPowerPerUnit;
    [ShowIf("showScalingSettings")]
    public float additionalCooldownReductionPerUnit;
    
    public float finalStrength {  get { return strength + bonusStrength; } }
    public float finalAgility {  get { return agility + bonusAgility; } }
    public float finalIntelligence { get { return intelligence + bonusIntelligence; } }
    

    public float finalMaximumHealth { get { return baseMaximumHealth + finalStrength * additionalAttackDamagePerUnit + bonusMaximumHealth; } }
    public float finalHealthRegenerationPerSecond { get { return baseHealthRegenerationPerSecond + finalStrength * additionalHealthRegenerationPerSecondPerUnit + bonusHealthRegenerationPerSecond; } }
    public float finalMaximumMana { get { return baseMaximumMana + finalIntelligence * additionalMaximumManaPerUnit + bonusMaximumMana; } }
    public float finalManaRegenerationPerSecond { get { return baseManaRegenerationPerSecond + finalIntelligence * additionalManaRegenerationPerSecondPerUnit + bonusManaRegenerationPerSecond; } }
    public float finalMovementSpeed { get { return (baseMovementSpeed + finalAgility * additionalMovementSpeedPerUnit + bonusMovementSpeed); } }
    public float finalAttackDamage { get { return baseAttackDamage + finalStrength * additionalAttackDamagePerUnit + bonusAttackDamage; } }
    public float finalAttacksPerSecond { get { return baseAttacksPerSecond * (1 + (finalAgility * additionalAttackSpeedPercentagePerUnit / 100) + (bonusAttackSpeedPercentage / 100)); } }
    public float finalSpellPower { get { return baseSpellPower + finalIntelligence * additionalSpellPowerPerUnit + bonusSpellPower; } }
    public float finalCooldownReduction { get { return baseCooldownReduction + finalIntelligence * additionalCooldownReductionPerUnit + bonusCooldownReduction; } }
    public float finalDodgeChance { get { return baseDodgeChance + finalAgility * additionalDodgeChancePerUnit + bonusDodgeChance; } }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
    }
    public void OnEvent(EventData photonEvent)
    {
        byte code = photonEvent.Code;

        if(code == NetworkingManager.event_SyncAllStats && photonView.IsMine)
        {
            SyncChangingStats();
            SyncBaseStats();
            SyncSecondaryStats();
            SyncTemporaryAttributes();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        currentHealth = Mathf.MoveTowards(currentHealth, finalMaximumHealth, finalHealthRegenerationPerSecond * Time.deltaTime);
        currentMana = Mathf.MoveTowards(currentMana, finalMaximumMana, finalManaRegenerationPerSecond * Time.deltaTime);
    }

    public void ValidateHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, finalMaximumHealth);
        if (isDead) currentHealth = 0f;
    }

    public void ValidateMana()
    {
        currentMana = Mathf.Clamp(currentMana, 0, finalMaximumMana);
    }

    public void SyncChangingStats()
    {
        float[] stats = { currentHealth, currentMana };
        photonView.RPC("RpcSyncChangingStats", RpcTarget.Others, stats);
    }



    public void SyncBaseStats()
    {
        float[] stats =
        {
            baseMaximumHealth, baseHealthRegenerationPerSecond,
            baseMaximumMana, baseManaRegenerationPerSecond,
            baseMovementSpeed,
            baseAttackDamage, baseAttacksPerSecond,
            baseSpellPower,
            baseCooldownReduction,
            baseDodgeChance
        };
        photonView.RPC("RpcSyncBaseStats", RpcTarget.Others, stats);
    }

    public void SyncSecondaryStats()
    {
        float[] stats = { strength, agility, intelligence };
        photonView.RPC("RpcSyncSecondaryStats", RpcTarget.Others, stats);
    }

    public void SyncTemporaryAttributes()
    {
        float[] stats =
        {
            bonusMaximumHealth, bonusHealthRegenerationPerSecond,
            bonusMaximumMana, bonusManaRegenerationPerSecond,
            bonusMovementSpeed,
            bonusAttackDamage, bonusAttackSpeedPercentage,
            bonusSpellPower,
            bonusCooldownReduction,
            bonusDodgeChance,
            bonusStrength, bonusAgility, bonusIntelligence
        };
        photonView.RPC("RpcSyncTemporaryAttributes", RpcTarget.Others, stats);
    }

    [PunRPC]
    private void RpcSyncChangingStats(float[] stats)
    {
        currentHealth = stats[0];
        currentMana = stats[1];
    }

    [PunRPC]
    private void RpcSyncBaseStats(float[] stats)
    {
        baseMaximumHealth = stats[0];
        baseHealthRegenerationPerSecond = stats[1];
        baseMaximumMana = stats[2];
        baseManaRegenerationPerSecond = stats[3];
        baseMovementSpeed = stats[4];
        baseAttackDamage = stats[5];
        baseAttacksPerSecond = stats[6];
        baseSpellPower = stats[7];
        baseCooldownReduction = stats[8];
        baseDodgeChance = stats[9];
    }

    [PunRPC]
    public void RpcSyncSecondaryStats(float[] stats)
    {
        strength = stats[0];
        agility = stats[1];
        intelligence = stats[2];
    }

    [PunRPC]
    public void RpcSyncTemporaryAttributes(float[] stats)
    {
        bonusMaximumHealth = stats[0];
        bonusHealthRegenerationPerSecond = stats[1];
        bonusMaximumMana = stats[2];
        bonusManaRegenerationPerSecond = stats[3];
        bonusMovementSpeed = stats[4];
        bonusAttackDamage = stats[5];
        bonusAttackSpeedPercentage = stats[6];
        bonusSpellPower = stats[7];
        bonusCooldownReduction = stats[8];
        bonusDodgeChance = stats[9];
        bonusStrength = stats[10];
        bonusAgility = stats[11];
        bonusIntelligence = stats[12];
    }
}
