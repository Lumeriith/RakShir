using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine.Events;


public enum CommandType { Move, Attack, AttackMove, Chase, AutoChase, Ability }





[System.Serializable]
public class Channel
{
    public SelfValidator channelValidator;
    
    public float duration;

    public bool canMove;
    public bool canAttack;
    public bool canUseAbility;

    public bool canBeCanceledByCaster;
    
    public UnityAction finishedCallback = null;
    public UnityAction canceledCallback = null;

    

    private bool hasEnded = false;

    public Channel(SelfValidator channelValidator, float duration, bool canMove, bool canAttack, bool canUseAbility, bool canBeCanceledByCaster, UnityAction finishedCallback, UnityAction canceledCallback)
    {
        this.channelValidator = channelValidator;
        this.duration = duration;
        this.canMove = canMove;
        this.canAttack = canAttack;
        this.canUseAbility = canUseAbility;
        this.canBeCanceledByCaster = canBeCanceledByCaster;
        this.finishedCallback = finishedCallback;
        this.canceledCallback = canceledCallback;
    }

    public void Tick()
    {
        if (hasEnded) return;
        duration = Mathf.MoveTowards(duration, 0, Time.deltaTime);
        if(duration == 0)
        {
            hasEnded = true;
            if (finishedCallback != null) finishedCallback.Invoke();
        }
    }
    public bool HasEnded()
    {
        return hasEnded;
    }

    public void Cancel()
    {
        if (hasEnded) return;
        duration = 0;
        hasEnded = true;
        if (canceledCallback != null) canceledCallback.Invoke();
    }
}

public class Command
{
    public CommandType type;
    public object[] parameters;

    private LivingThing self;

    private float lastAttackMoveCheckTime = -1f;
    private float autoChaseOutOfRangeTime = 0f;

    public Command(LivingThing self, CommandType type, params object[] parameters)
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
                return ProcessAttack((LivingThing)parameters[0]);
            case CommandType.AttackMove:
                return ProcessAttackMove((Vector3)parameters[0]);
            case CommandType.Chase:
                return ProcessChase((LivingThing)parameters[0]);
            case CommandType.AutoChase:
                return ProcessAutoChase((LivingThing)parameters[0]);
            case CommandType.Ability:
                return ProcessAbility((AbilityTrigger)parameters[0], (CastInfo)parameters[1]);
        }

        return false;
    }

    private bool ProcessMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
        self.control.agent.destination = destination;
        if (self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessAttack(LivingThing target)
    {
        if (self.control.skillSet[0] == null) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) > self.control.skillSet[0].range) return true;
        if (!self.control.skillSet[0].selfValidator.Evaluate(self)) return true;
        if (!self.control.skillSet[0].targetValidator.Evaluate(self, target)) return true;
        if (!self.control.skillSet[0].isCooledDown) return true;
        
        CastInfo info = new CastInfo { owner = self, directionVector = Vector3.zero, point = Vector3.zero, target = target };
        self.control.skillSet[0].Cast(info);
        return true;
    }

    private bool ProcessAttackMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
        if (self.control.skillSet[0] == null)
        {
            return ProcessMove(destination);
        }

        if(lastAttackMoveCheckTime < 0 || Time.time - lastAttackMoveCheckTime >= 1f / lastAttackMoveCheckTime)
        {
            lastAttackMoveCheckTime = Time.time;
            List<LivingThing> targets = self.GetAllTargetsInRange(self.transform.position, self.control.attackMoveTargetCheckRange, self.control.skillSet[0].targetValidator);
            if(targets.Count == 0)
            {
                self.control.agent.destination = destination;
                return false;
            }
            else
            {
                type = CommandType.Chase;
                parameters[0] = targets[0];
                return false;
            }
        }

        if (self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessChase(LivingThing target)
    {
        if (SelfValidator.CancelsChaseCommand.Evaluate(self)) return true;
        if (target == null || target.IsDead()) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) <= self.control.skillSet[0].range)
        {
            self.control.agent.destination = self.transform.position;
            ProcessAttack(target);
        }
        else
        {
            self.control.agent.destination = target.transform.position;
        }

        return false;
    }

    private bool ProcessAutoChase(LivingThing target)
    {
        if(Vector3.Distance(self.transform.position,target.transform.position) > self.control.autoChaseRange)
        {
            autoChaseOutOfRangeTime += Time.deltaTime;
        }
        else
        {
            autoChaseOutOfRangeTime = 0f;
        }
        
        if (autoChaseOutOfRangeTime >= self.control.autoChaseOutOfRangeCancelTime) return true;
        
        return ProcessChase(target);
    }

    private bool ProcessAbility(AbilityTrigger trigger, CastInfo info)
    {
        if (!trigger.isCooledDown) return true;
        if (!trigger.selfValidator.Evaluate(self)) return true;

        switch (trigger.targetingType)
        {
            case AbilityTrigger.TargetingType.None:
                self.control.agent.destination = self.transform.position;
                trigger.Cast(info);
                return true;
            case AbilityTrigger.TargetingType.Target:
                if (!trigger.targetValidator.Evaluate(self, info.target)) return true;
                if (Vector3.Distance(self.transform.position, info.target.transform.position) > trigger.range)
                {
                    self.control.agent.destination = info.target.transform.position;
                    return false;
                }
                else
                {
                    self.control.agent.destination = self.transform.position;
                    trigger.Cast(info);
                    return true;
                }
            case AbilityTrigger.TargetingType.Direction:
                trigger.Cast(info);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(self.transform.position, info.point) > trigger.range)
                {
                    self.control.agent.destination = info.point;
                    return false;
                }
                else
                {
                    self.control.agent.destination = self.transform.position;
                    trigger.Cast(info);
                    return true;
                }
            case AbilityTrigger.TargetingType.PointNonStrict:
                if(Vector3.Distance(self.transform.position, info.point) > trigger.range)
                {
                    info.point = self.transform.position + (info.point - self.transform.position).normalized * trigger.range;
                }
                trigger.Cast(info);
                return true;
        }

        return false;
    }

    
}



public class LivingThingControl : MonoBehaviourPun
{
    public NavMeshAgent agent { get; private set; }

    public AbilityTrigger[] skillSet = new AbilityTrigger[6];

    public float attackMoveTargetChecksForSecond = 4f;
    public float attackMoveTargetCheckRange = 4f;

    public float autoChaseRange = 4f;
    public float autoChaseOutOfRangeCancelTime = 2f;
    public float angularSpeed = 600f;

    private LivingThing livingThing;

    private List<Command> reservedCommands = new List<Command>();
    private Command currentCommand { get { return reservedCommands.Count == 0 ? null : reservedCommands[0]; } }
    private List<Channel> ongoingChannels = new List<Channel>();
    private Quaternion desiredRotation = Quaternion.identity;


    #region Commands
    public void CommandStop()
    {
        List<Channel> removalFlaggedChannels = new List<Channel>();
        reservedCommands.Clear();
        
        foreach (Channel channel in ongoingChannels)
        {
            if(channel.canBeCanceledByCaster)
            {
                removalFlaggedChannels.Add(channel);
            }
        }

        foreach(Channel channel in removalFlaggedChannels)
        {
            channel.Cancel();
            ongoingChannels.Remove(channel);
        }
    }

    public void CommandMove(Vector3 destination, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Move, destination);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAttack(LivingThing target, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Attack, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAttackMove(Vector3 destination, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.AttackMove, destination);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandChase(LivingThing target, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Chase, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAutoChase(LivingThing target, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.AutoChase, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAbility(AbilityTrigger trigger, CastInfo info, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Ability, trigger, info);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    #endregion Commands
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    public void LookAt(Vector3 location, bool immediately = false)
    {
        desiredRotation = Quaternion.LookRotation(location - transform.position, transform.up);
        if (immediately)
        {
            transform.rotation = desiredRotation;
        }
    }

    public void StartChanneling(Channel channel, bool multiplyAttackIntervalToDuration = false)
    {
        if (multiplyAttackIntervalToDuration)
        {
            channel.duration *= (1f / livingThing.stat.finalAttacksPerSecond) / ((100f + livingThing.statusEffect.totalHasteAmount) / 100f);
        }
        ongoingChannels.Add(channel);
    }
    

    private void Update()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, angularSpeed * Time.deltaTime);

        List<Channel> channelsToRemove = new List<Channel>();


        foreach(Channel channel in ongoingChannels)
        {
            if (channel.HasEnded())
            {
                channelsToRemove.Add(channel);
            }
            else
            {
                channel.Tick();
            }
        }

        foreach(Channel channel in channelsToRemove)
        {
            ongoingChannels.Remove(channel);
        }
        

        if(currentCommand != null)
        {
            bool isCommandFinished = currentCommand.Process();
            if (isCommandFinished) reservedCommands.RemoveAt(0);
        }
    }



}
