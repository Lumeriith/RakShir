using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetValidator : System.ICloneable
{

    public bool canTargetSelf = false;
    public bool canTargetOwnSummon = false;

    public bool canTargetAlliedPlayer = false;
    public bool canTargetAlliedSummon = false;
    public bool canTargetAlliedMonster = false;

    public bool canTargetEnemyPlayer = false;
    public bool canTargetEnemySummon = false;
    public bool canTargetEnemyMonster = true;

    public List<CoreStatusEffectType> excludes = new List<CoreStatusEffectType>() { CoreStatusEffectType.Stasis, CoreStatusEffectType.Untargetable };

    public object Clone()
    {
        TargetValidator tv = new TargetValidator();
        tv.canTargetSelf = canTargetSelf;
        tv.canTargetOwnSummon = canTargetOwnSummon;
        tv.canTargetAlliedPlayer = canTargetAlliedPlayer;
        tv.canTargetAlliedSummon = canTargetAlliedSummon;
        tv.canTargetAlliedMonster = canTargetAlliedMonster;
        tv.canTargetEnemyPlayer = canTargetEnemyPlayer;
        tv.canTargetEnemySummon = canTargetEnemySummon;
        tv.canTargetEnemyMonster = canTargetEnemyMonster;
        tv.excludes = new List<CoreStatusEffectType>(excludes);
        return tv;
    }


    public static TargetValidator HarmfulSpellDefault = new TargetValidator
    {
        canTargetSelf = false,
        canTargetOwnSummon = false,
        canTargetAlliedPlayer = false,
        canTargetAlliedSummon = false,
        canTargetAlliedMonster = false,
        canTargetEnemyPlayer = true,
        canTargetEnemySummon = true,
        canTargetEnemyMonster = true,

        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stasis,
          CoreStatusEffectType.Invulnerable,
          CoreStatusEffectType.Untargetable }
    };

    public static TargetValidator BeneficialSpellDefault = new TargetValidator
    {
        canTargetSelf = true,
        canTargetOwnSummon = true,
        canTargetAlliedPlayer = true,
        canTargetAlliedSummon = true,
        canTargetAlliedMonster = true,
        canTargetEnemyPlayer = false,
        canTargetEnemySummon = false,
        canTargetEnemyMonster = false,

        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stasis,
          CoreStatusEffectType.Untargetable }
    };


    public bool Evaluate(LivingThing self, LivingThing target)
    {
        if (target == null || self == null) return false;

        bool isSelf = target == self;
        bool isAlly = !isSelf && self.team == target.team && self.team != Team.None;
        bool isOwn = target.type == LivingThingType.Summon ? target.summoner == self : false;
        bool isEnemy = !isSelf && !isOwn && !isAlly && ((self.team == Team.None && target.team == Team.None) || (self.team != target.team));
        bool isMonster = target.type == LivingThingType.Monster;
        bool isPlayer = target.type == LivingThingType.Player;
        bool isSummon = target.type == LivingThingType.Summon;

        bool typeAndTeamCheck = false;

        if (canTargetSelf && isSelf) typeAndTeamCheck = true;
        else if (canTargetOwnSummon && isOwn && isSummon) typeAndTeamCheck = true;
        else if (canTargetAlliedMonster && isAlly && isMonster) typeAndTeamCheck = true;
        else if (canTargetAlliedPlayer && isAlly && isPlayer) typeAndTeamCheck = true;
        else if (canTargetAlliedSummon && isAlly && isSummon) typeAndTeamCheck = true;
        else if (canTargetEnemyPlayer && isEnemy && isPlayer) typeAndTeamCheck = true;
        else if (canTargetEnemySummon && isEnemy && isSummon) typeAndTeamCheck = true;
        else if (canTargetEnemyMonster && isEnemy && isMonster) typeAndTeamCheck = true;

        if (!typeAndTeamCheck) return false;

        foreach(CoreStatusEffectType type in excludes)
        {
            if (target.statusEffect.IsAffectedBy(type)) return false;
        }

        return true;
    }

}


[System.Serializable]
public class SelfValidator : System.ICloneable
{

    public List<CoreStatusEffectType> excludes = new List<CoreStatusEffectType>();

    public static SelfValidator CanTick = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stasis }
    };

    public static SelfValidator CanHaveMoveSpeedOverZero = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Root }
    };

    public static SelfValidator CanCommandMove = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>() {
          CoreStatusEffectType.Stun,
          CoreStatusEffectType.Airborne,
          CoreStatusEffectType.Sleep,
          CoreStatusEffectType.Root,
          CoreStatusEffectType.MindControl,
          CoreStatusEffectType.Charm,
          CoreStatusEffectType.Fear }
    };

    public static SelfValidator CanHaveMoveActionReserved = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stun,
          CoreStatusEffectType.Airborne,
          CoreStatusEffectType.Sleep }
    };

    public static SelfValidator CanBeDamaged = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stasis,
          CoreStatusEffectType.Invulnerable,
          CoreStatusEffectType.Protected }
    };

    public static SelfValidator CanHaveHarmfulCoreStatusEffects = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Invulnerable,
          CoreStatusEffectType.Unstoppable }
    };

    public static SelfValidator CanHaveNavMeshEnabled = new SelfValidator
    {
        excludes = new List<CoreStatusEffectType>()
        { CoreStatusEffectType.Stasis,
          CoreStatusEffectType.Airborne }
    };


    public object Clone()
    {
        SelfValidator sv = new SelfValidator();
        sv.excludes = new List<CoreStatusEffectType>(excludes);
        return sv;
    }


    public bool Evaluate(LivingThing self)
    {
        if (self == null) return false;

        foreach (CoreStatusEffectType type in excludes)
        {
            if (self.statusEffect.IsAffectedBy(type)) return false;
        }

        return true;
    }

}



