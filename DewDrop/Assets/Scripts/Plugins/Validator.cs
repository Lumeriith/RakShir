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

    public bool canTargetEnemyPlayer = true;
    public bool canTargetEnemySummon = true;
    public bool canTargetEnemyMonster = true;

    public bool evaluatesFalseIfDead = true;

    public List<StatusEffectType> excludes = new List<StatusEffectType>() { StatusEffectType.Stasis, StatusEffectType.Invulnerable, StatusEffectType.Untargetable };

    public bool invertResult = false;

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
        tv.excludes = new List<StatusEffectType>(excludes);
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

        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis,
          StatusEffectType.Invulnerable,
          StatusEffectType.Untargetable }
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

        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis,
          StatusEffectType.Untargetable }
    };


    public bool Evaluate(LivingThing self, LivingThing target)
    {
        if (target == null || self == null || (evaluatesFalseIfDead && target.IsDead())) return invertResult ? true : false;

        Relation relation = self.GetRelationTo(target);
        bool isSelf = target == self;
        bool isAlly = relation == Relation.Ally;
        bool isOwn = relation == Relation.Own;
        bool isEnemy = relation == Relation.Enemy;
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

        if (!typeAndTeamCheck) return invertResult ? true : false;

        foreach(StatusEffectType type in excludes)
        {
            if (target.statusEffect.IsAffectedBy(type)) return invertResult ? true : false;
        }

        return invertResult ? false : true;
    }

}


[System.Serializable]
public class SelfValidator : System.ICloneable
{
    public bool evaluatesFalseIfDead = true;
    public List<StatusEffectType> excludes = new List<StatusEffectType>() { StatusEffectType.Stun, StatusEffectType.Airborne, StatusEffectType.Sleep, StatusEffectType.Polymorph, StatusEffectType.MindControl, StatusEffectType.Charm, StatusEffectType.Fear, StatusEffectType.Silence };

    public bool invertResult = false;

    public static SelfValidator CanTick = new SelfValidator
    {
        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis },
        evaluatesFalseIfDead = false
    };


    public static SelfValidator CancelsMoveCommand = new SelfValidator
    {
        excludes = new List<StatusEffectType>() {
            StatusEffectType.Stun,
            StatusEffectType.Airborne,
            StatusEffectType.Sleep,
            StatusEffectType.Root,
            StatusEffectType.MindControl,
            StatusEffectType.Charm,
            StatusEffectType.Fear,
            StatusEffectType.Dash },
        evaluatesFalseIfDead = false,
        invertResult = true
    };
    public static SelfValidator CancelsChaseCommand = new SelfValidator
    {
        excludes = new List<StatusEffectType>() {
            StatusEffectType.Stun,
            StatusEffectType.Airborne,
            StatusEffectType.Sleep,
            StatusEffectType.MindControl,
            StatusEffectType.Charm,
            StatusEffectType.Fear,
            StatusEffectType.MindControl },
        evaluatesFalseIfDead = false,
        invertResult = true
    };





    public static SelfValidator CanBeDamaged = new SelfValidator
    {
        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis,
          StatusEffectType.Invulnerable,
          StatusEffectType.Protected }
    };

    public static SelfValidator CanHaveHarmfulStatusEffects = new SelfValidator
    {
        excludes = new List<StatusEffectType>()
        { StatusEffectType.Invulnerable,
          StatusEffectType.Unstoppable }
    };




    public object Clone()
    {
        SelfValidator sv = new SelfValidator();
        sv.excludes = new List<StatusEffectType>(excludes);
        return sv;
    }


    public bool Evaluate(LivingThing self)
    {
        if (self == null || (evaluatesFalseIfDead && self.IsDead())) return invertResult ? true : false;

        foreach (StatusEffectType type in excludes)
        {
            if (self.statusEffect.IsAffectedBy(type)) return invertResult ? true : false;
        }

        return invertResult ? false : true;
    }

}



