using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public struct InfoManaSpent
{
    public LivingThing livingThing;
    public float amount;
}
public struct InfoDeath
{
    public LivingThing victim;
    public LivingThing killer;
}
public struct InfoDamage
{
    public LivingThing to;
    public LivingThing from;
    public float damage;
    public DamageType type;
}
public struct InfoMagicDamage
{
    public LivingThing to;
    public LivingThing from;
    public float originalDamage;
    public float finalDamage;
}
public struct InfoHeal
{
    public LivingThing to;
    public LivingThing from;
    public float originalHeal;
    public float finalHeal;
}
public struct InfoManaHeal
{
    public LivingThing to;
    public LivingThing from;
    public float originalManaHeal;
    public float finalManaHeal;
}
public struct InfoBasicAttackHit
{
    public LivingThing to;
    public LivingThing from;
    public float damage;
}
public struct InfoMiss
{
    public LivingThing to;
    public LivingThing from;
}
public struct InfoChannel
{
    public LivingThing livingThing;
    public float remainingTime;
}
public struct InfoGold
{
    public LivingThing from;
    public LivingThing to;
    public float amount;
}
public struct InfoSpendGold
{
    public LivingThing livingThing;
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

public interface IDewActionCaller
{
    LivingThing invokerEntity { get; }

    System.Action<InfoManaSpent> OnSpendMana { get; set; }
    System.Action<InfoDamage> OnDealDamage { get; set; }
    System.Action<InfoDamage> OnDealPureDamage { get; set; }
    System.Action<InfoMagicDamage> OnDealMagicDamage { get; set; }
    System.Action<InfoBasicAttackHit> OnDoBasicAttackHit { get; set; }
    System.Action<InfoHeal> OnDoHeal { get; set; }
    System.Action<InfoManaHeal> OnDoManaHeal { get; set; }

    int Serialize();
}


