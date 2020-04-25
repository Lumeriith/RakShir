using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public struct InfoManaSpent
{
    public Entity livingThing;
    public float amount;
}
public struct InfoDeath
{
    public Entity victim;
    public Entity killer;
}
public struct InfoDamage
{
    public Entity to;
    public Entity from;
    public float damage;
    public DamageType type;
}
public struct InfoMagicDamage
{
    public Entity to;
    public Entity from;
    public float originalDamage;
    public float finalDamage;
}
public struct InfoHeal
{
    public Entity to;
    public Entity from;
    public float originalHeal;
    public float finalHeal;
}
public struct InfoManaHeal
{
    public Entity to;
    public Entity from;
    public float originalManaHeal;
    public float finalManaHeal;
}
public struct InfoBasicAttackHit
{
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
    public Entity livingThing;
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
    public Entity livingThing;
    public float amount;
}

public delegate void ManaSpent(InfoManaSpent info);
public delegate void DamageHandler(InfoDamage info);
public delegate void MagicDamageHandler(InfoMagicDamage info);
public delegate void HealHandler(InfoHeal info);
public delegate void ManaHealHandler(InfoManaHeal info);
public delegate void BasicAttackHitHandler(InfoBasicAttackHit info);
public delegate void ChannelHandler(InfoChannel info);
public delegate void GoldHandler(InfoGold info);
public delegate void SpendGoldHandler(InfoGold info);



