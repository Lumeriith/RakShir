using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingThing : MonoBehaviour
{
    [Header("Health Value")]
    public float maxHp;

    [SerializeField]
    private float currentHp;

    private void Awake()
    {
        currentHp = maxHp;
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
