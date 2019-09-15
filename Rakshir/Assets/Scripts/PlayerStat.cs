using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlayerStat : MonoBehaviour
{
    [ShowNativeProperty]
    public int strength
    {
        set
        {
            m_strength = value;
            hp += hpPerStr * value;
            // Player.UpdateMaxHp(hp);
            atk += atkPerStr * value;
        }

        get
        {
            return m_strength;
        }
    }
    private int m_strength;

    [ShowNativeProperty]
    public int wisdom
    {
        set
        {
            m_wisdom = value;
            cooldownReduce += cooldownReducePerWis * value;
            skillAtk += skillAtkPerWis * value;
        }

        get
        {
            return m_wisdom;
        }
    }
    private int m_wisdom;

    [ShowNativeProperty]
    public int dexterity
    {
        set
        {
            m_dexterity = value;
            runSpeed += runSpeedPerDex * value;
            atkSpeed += atkSpeedPerDex * value;
        }

        get
        {
            return m_dexterity;
        }
    }
    private int m_dexterity;

    [BoxGroup("Per Strength")]
    [SerializeField]
    private float hpPerStr;

    [BoxGroup("Per Strength")]
    [SerializeField]
    private float atkPerStr;

    [BoxGroup("Per Wisdom")]
    [SerializeField]
    private float cooldownReducePerWis;

    [BoxGroup("Per Wisdom")]
    [SerializeField]
    private float skillAtkPerWis;

    [BoxGroup("Per Dexterity")]
    [SerializeField]
    private float runSpeedPerDex;

    [BoxGroup("Per Dexterity")]
    [SerializeField]
    private float atkSpeedPerDex;

    [Space(10)]

    [Header("Current Stat")]
    public float hp;
    public float atk;
    public float cooldownReduce;
    public float maxCooldownReduce;
    public float skillAtk;
    public float runSpeed;
    public float atkSpeed;


    private void Awake()
    {
        // hp = Player.maxHp;
    }
}
