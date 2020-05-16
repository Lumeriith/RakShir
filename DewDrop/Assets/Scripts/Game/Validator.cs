using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetValidatorPreset
{
    Custom = 0,
    Beneficial = 1,
    Harmful = 2
}

[System.Serializable]
public class TargetValidator
{
    [SerializeField, HideLabel, EnumToggleButtons]
    private TargetValidatorPreset _preset = TargetValidatorPreset.Custom;

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetSelf = false;
    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetOwnSummon = false;

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetAlliedPlayer = false;
    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetAlliedSummon = false;
    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetAlliedMonster = false;

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetEnemyPlayer = true;
    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetEnemySummon = true;
    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool canTargetEnemyMonster = true;

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool evaluatesFalseIfDead = true;

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public List<StatusEffectType> excludes = new List<StatusEffectType>() { StatusEffectType.Stasis, StatusEffectType.Invulnerable, StatusEffectType.Untargetable };

    [ShowIf("_preset", Value = TargetValidatorPreset.Custom)]
    public bool invertResult = false;

    public static TargetValidator HarmfulPreset = new TargetValidator
    {
        _preset = TargetValidatorPreset.Custom,
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

    public static TargetValidator BeneficialPreset = new TargetValidator
    {
        _preset = TargetValidatorPreset.Custom,
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

    public bool Evaluate(Entity self, Entity target)
    {
        if (_preset == TargetValidatorPreset.Beneficial) return BeneficialPreset.Evaluate(self, target);
        if (_preset == TargetValidatorPreset.Harmful) return HarmfulPreset.Evaluate(self, target);

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

public enum SelfValidatorPreset
{
    Custom = 0,
    Default = 1,
    Movement = 2
}

[System.Serializable]
public class SelfValidator
{
    [SerializeField, HideLabel, EnumToggleButtons]
    private SelfValidatorPreset _preset = SelfValidatorPreset.Custom;

    [ShowIf("_preset", Value = SelfValidatorPreset.Custom)]
    public bool evaluatesFalseIfDead = true;
    [ShowIf("_preset", Value = SelfValidatorPreset.Custom)]
    public List<StatusEffectType> excludes = new List<StatusEffectType>() {
        StatusEffectType.Stun, 
        StatusEffectType.Airborne,
        StatusEffectType.Dash,
        StatusEffectType.Sleep,
        StatusEffectType.Polymorph,
        StatusEffectType.MindControl,
        StatusEffectType.Charm,
        StatusEffectType.Fear,
        StatusEffectType.Silence
    };
    [ShowIf("_preset", Value = SelfValidatorPreset.Custom)]
    public bool invertResult = false;

    public static SelfValidator DefaultPreset = new SelfValidator()
    {
        _preset = SelfValidatorPreset.Custom,
        excludes = new List<StatusEffectType>()
        {
            StatusEffectType.Stun,
            StatusEffectType.Airborne,
            StatusEffectType.Dash,
            StatusEffectType.Sleep,
            StatusEffectType.Polymorph,
            StatusEffectType.MindControl,
            StatusEffectType.Charm,
            StatusEffectType.Fear,
            StatusEffectType.Silence
        },
        evaluatesFalseIfDead = true,
        invertResult = false
    };
    public static SelfValidator MovementPreset = new SelfValidator()
    {
        _preset = SelfValidatorPreset.Custom,
        excludes = new List<StatusEffectType>()
        {
            StatusEffectType.Stun,
            StatusEffectType.Airborne,
            StatusEffectType.Dash,
            StatusEffectType.Sleep,
            StatusEffectType.Polymorph,
            StatusEffectType.MindControl,
            StatusEffectType.Charm,
            StatusEffectType.Fear,
            StatusEffectType.Silence,
            StatusEffectType.Root
        },
        evaluatesFalseIfDead = true,
        invertResult = false
    };

    public static SelfValidator CanTick = new SelfValidator
    {
        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis },
        evaluatesFalseIfDead = false
    };

    public static SelfValidator CanBePushed = new SelfValidator
    {
        excludes = new List<StatusEffectType>()
        { StatusEffectType.Stasis,
          StatusEffectType.Unstoppable,
          StatusEffectType.Invulnerable,
          StatusEffectType.Untargetable},
        evaluatesFalseIfDead = true
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

    public bool Evaluate(Entity self)
    {
        if (_preset == SelfValidatorPreset.Default) return DefaultPreset.Evaluate(self);
        if (_preset == SelfValidatorPreset.Movement) return MovementPreset.Evaluate(self);

        if (self == null || (evaluatesFalseIfDead && self.IsDead())) return invertResult ? true : false;

        foreach (StatusEffectType type in excludes)
        {
            if (self.statusEffect.IsAffectedBy(type)) return invertResult ? true : false;
        }

        return invertResult ? false : true;
    }

}



