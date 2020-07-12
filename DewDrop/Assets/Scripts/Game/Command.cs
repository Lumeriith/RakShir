using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public CommandType type;
    public object[] parameters;

    private Entity self;

    private float lastAttackMoveCheckTime = -1f;
    private float autoChaseOutOfRangeTime = 0f;

    public Command(Entity self, CommandType type, params object[] parameters)
    {
        this.self = self;
        this.type = type;
        this.parameters = parameters;
    }





    // returns true when the command is done and finished, returns false if the command needs another tick.
    public bool Process()
    {
        switch (type)
        {
            case CommandType.Move:
                return ProcessMove((Vector3)parameters[0]);
            case CommandType.Attack:
                return ProcessAttack((Entity)parameters[0]);
            case CommandType.AttackMove:
                return ProcessAttackMove((Vector3)parameters[0]);
            case CommandType.Chase:
                return ProcessChase((Entity)parameters[0]);
            case CommandType.AutoChase:
                return ProcessAutoChase((Entity)parameters[0]);
            case CommandType.Cast:
                return ProcessCastable((AbilityTrigger)parameters[0], (CastInfo)parameters[1]);
            case CommandType.Activate:
                return ProcessActivate((Activatable)parameters[0]);
            case CommandType.AutoAttackInRange:
                return ProcessAutoAttackInRange((Entity)parameters[0]);
        }
        return false;
    }

    private bool ProcessActivate(Activatable target)
    {
        if (target == null) return true;
        if (!target.channel.channelValidator.Evaluate(self)) return true;
        Item item = target as Item;
        if (item != null && item.owner != null) return true;

        if (Vector3.Distance(self.transform.position, target.transform.position) <= target.activationRange)
        {
            self.control.agentDestination = self.transform.position;
            self.LookAt(target.transform.position);
            target.StartActivate(self);
            return true;
        }
        else
        {
            if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
            if (self.control.IsMoveProhibitedByChannel(true)) return false;
            self.control.agentDestination = target.transform.position;
            if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
            {
                self.LookAt(self.control.agent.path.corners[1]);
            }
        }

        return false;
    }

    private bool ProcessMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
        if (self.control.IsMoveProhibitedByChannel(true)) return false;
        self.control.agentDestination = destination;
        Vector3 temp = destination - self.transform.position;
        temp.y = 0f;
        if (temp.magnitude < 0.1f && self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
        {
            self.LookAt(self.control.agent.path.corners[1]);
        }
        return false;
    }

    private bool ProcessAttack(Entity target)
    {
        if (target.IsDead()) return true;
        self.LookAt(target.transform.position);
        if (self.control.skillSet[0] == null) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) - target.unitRadius > self.control.skillSet[0].castMethod.range) return true;
        if (!((ICastable)self.control.skillSet[0]).IsReady()) return true;
        CastInfo info = new CastInfo { owner = self, directionVector = Vector3.zero, point = Vector3.zero, target = target };
        if (!((ICastable)self.control.skillSet[1]).IsCastValid(info)) return true;
        if (self.control.IsAttackProhibitedByChannel()) return true;

        self.control.skillSet[0].Cast(info, (1 / self.stat.finalAttacksPerSecond) / (1f + self.statusEffect.status.haste / 100f));
        return true;
    }

    private bool ProcessAttackMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
        if (self.control.skillSet[0] == null)
        {
            return ProcessMove(destination);
        }

        if (lastAttackMoveCheckTime < 0 || Time.time - lastAttackMoveCheckTime >= 1f / self.control.attackMoveTargetChecksForSecond)
        {
            lastAttackMoveCheckTime = Time.time;
            List<Entity> targets = self.GetAllTargetsInRange(self.transform.position, Mathf.Max(self.control.skillSet[0].castMethod.range, 6f), self.control.skillSet[0].targetValidator);
            if (targets.Count == 0)
            {
                if (self.control.IsMoveProhibitedByChannel(true)) return false;
                self.control.agentDestination = destination;
                return false;
            }
            else
            {
                type = CommandType.Chase;
                parameters[0] = targets[0];
                return false;
            }
        }

        if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
        {
            self.LookAt(self.control.agent.path.corners[1]);
        }

        if (self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessChase(Entity target)
    {

        if (SelfValidator.CancelsChaseCommand.Evaluate(self)) return true;
        if (target == null || target.IsDead()) return true;

        if (self.control.skillSet[0] == null) return true;
        if (!self.control.skillSet[0].selfValidator.Evaluate(self)) return true;
        if (!self.control.skillSet[0].targetValidator.Evaluate(self, target)) return true;

        if (self.currentRoom != target.currentRoom) return false;

        // if (!self.control.skillSet[0].isCooledDown || !self.control.skillSet[0].IsReady()) return false;

        if (Vector3.Distance(self.transform.position, target.transform.position) - target.unitRadius < self.control.skillSet[0].castMethod.range)
        {

            self.control.agentDestination = self.transform.position;
            ProcessAttack(target);
        }
        else
        {
            if (self.control.IsAttackProhibitedByChannel(false)) return false;
            if (self.control.IsMoveProhibitedByChannel(true)) return false;

            self.control.agentDestination = target.transform.position;
            if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
            {
                self.LookAt(self.control.agent.path.corners[1]);
            }
            else
            {
                self.LookAt(target.transform.position);
            }
        }

        return false;
    }

    private bool ProcessAutoChase(Entity target)
    {

        if (Vector3.Distance(self.transform.position, target.transform.position) - target.unitRadius > self.control.autoChaseRange)
        {
            autoChaseOutOfRangeTime += Time.deltaTime;
        }
        else
        {
            autoChaseOutOfRangeTime = 0f;
        }

        if (autoChaseOutOfRangeTime > self.control.autoChaseOutOfRangeCancelTime) return true;

        return ProcessChase(target);
    }

    private bool ProcessAutoAttackInRange(Entity target)
    {
        if (self.control.skillSet[0] == null) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) - target.unitRadius > self.control.skillSet[0].castMethod.range)
        {
            return true;
        }
        return ProcessChase(target);
    }


    private bool ProcessCastable(ICastable castable, CastInfo info)
    {
        if (self.control.IsAbilityProhibitedByChannel()) return false;
        if (!castable.IsReady()) return true;
        if (!castable.IsCastValid(info)) return true;

        if (info.target != null && self.currentRoom != info.target.currentRoom) return true;



        switch (castable.castMethod.type)
        {
            case CastMethodType.None:
                self.control.agentDestination = self.transform.position;
                castable.Cast(info);
                return true;
            case CastMethodType.Arrow:
                self.LookAt(self.transform.position + info.directionVector);
                self.control.agentDestination = self.transform.position;
                castable.Cast(info);
                return true;
            case CastMethodType.Cone:
                self.LookAt(self.transform.position + info.directionVector);
                self.control.agentDestination = self.transform.position;
                castable.Cast(info);
                return true;
            case CastMethodType.Target:
                if (Vector3.Distance(self.transform.position, info.target.transform.position) - info.target.unitRadius > castable.castMethod.range)
                {
                    self.control.agentDestination = info.target.transform.position;
                    if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
                    {
                        self.LookAt(self.control.agent.path.corners[1]);
                    }
                    return false;
                }
                else
                {
                    self.control.agentDestination = self.transform.position;
                    self.LookAt(info.target.transform.position);
                    castable.Cast(info);
                    return true;
                }
            case CastMethodType.Point:
                if (castable.castMethod.isClamping)
                {
                    if (Vector3.Distance(self.transform.position, info.point) > castable.castMethod.range)
                    {
                        self.control.agentDestination = info.point;
                        if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
                        {
                            self.LookAt(self.control.agent.path.corners[1]);
                        }
                        return false;
                    }
                    else
                    {
                        self.control.agentDestination = self.transform.position;
                        self.LookAt(info.point);
                        castable.Cast(info);
                        return true;
                    }
                }
                else
                {
                    if (Vector3.Distance(self.transform.position, info.point) > castable.castMethod.range)
                    {
                        info.point = self.transform.position + (info.point - self.transform.position).normalized * castable.castMethod.range;
                    }
                    self.control.agentDestination = self.transform.position;
                    self.LookAt(info.point);
                    castable.Cast(info);
                    return true;
                }
        }
        return false;
    }


}
