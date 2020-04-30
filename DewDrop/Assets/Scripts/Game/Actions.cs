using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public struct InfoAbilityInstance
{
    public Entity entity;
    public AbilityInstance instance;
}
public struct InfoManaSpent
{
    public DewActionCaller caller;
    public Entity entity;
    public float amount;
}
public struct InfoDeath
{
    public Entity victim;
    public Entity killer;
}
public struct InfoDamage
{
    public DewActionCaller caller;
    public Entity to;
    public Entity from;
    public float damage;
    public DamageType type;
}
public struct InfoMagicDamage
{
    public DewActionCaller caller;
    public Entity to;
    public Entity from;
    public float originalDamage;
    public float finalDamage;
}
public struct InfoHeal
{
    public DewActionCaller caller;
    public Entity to;
    public Entity from;
    public float originalHeal;
    public float finalHeal;
}
public struct InfoManaHeal
{
    public DewActionCaller caller;
    public Entity to;
    public Entity from;
    public float originalManaHeal;
    public float finalManaHeal;
}
public struct InfoBasicAttackHit
{
    public DewActionCaller caller;
    public Entity to;
    public Entity from;
    public float damage;
}
public struct InfoMiss
{
    public Entity to;
    public Entity from;
}
public struct InfoChannel
{
    public Entity entity;
    public float remainingTime;
}
public struct InfoGold
{
    public Entity from;
    public Entity to;
    public float amount;
}
public struct InfoSpendGold
{
    public Entity entity;
    public float amount;
}