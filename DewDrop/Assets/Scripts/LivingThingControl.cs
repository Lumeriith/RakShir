using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine.Events;


public enum CommandType { Move, Attack, AttackMove, Chase, AutoChase, Ability, Activate, Consumable }

public enum AIMode { None, AutoAttackInRange, AutoChaseToAttack }



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

[System.Serializable]
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
            case CommandType.Activate:
                return ProcessActivate((Activatable)parameters[0]);
            case CommandType.Consumable:
                return ProcessConsumable((Consumable)parameters[0], (CastInfo)parameters[1]);
        }
        return false;
    }

    private bool ProcessConsumable(Consumable consumable, CastInfo info)
    {
        if (self.control.IsAbilityProhibitedByChannel()) return false;
        if (!consumable.selfValidator.Evaluate(self)) return true;
        PlayerItemBelt belt = self.GetComponent<PlayerItemBelt>();

        switch (consumable.targetingType)
        {
            case AbilityTrigger.TargetingType.None:
                self.control.agentDestination = self.transform.position;
                belt.UseConsumable(consumable, info);
                return true;
            case AbilityTrigger.TargetingType.Target:
                if (!consumable.targetValidator.Evaluate(self, info.target)) return true;
                if (Vector3.Distance(self.transform.position, info.target.transform.position) > consumable.range)
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
                    belt.UseConsumable(consumable, info);
                    return true;
                }
            case AbilityTrigger.TargetingType.Direction:
                self.LookAt(self.transform.position + info.directionVector);
                belt.UseConsumable(consumable, info);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(self.transform.position, info.point) > consumable.range)
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
                    belt.UseConsumable(consumable, info);
                    return true;
                }
            case AbilityTrigger.TargetingType.PointNonStrict:
                if (Vector3.Distance(self.transform.position, info.point) > consumable.range)
                {
                    info.point = self.transform.position + (info.point - self.transform.position).normalized * consumable.range;
                }
                self.LookAt(info.point);
                belt.UseConsumable(consumable, info);
                return true;
        }

        return false;
    }


    private bool ProcessActivate(Activatable target)
    {
        if (!target.channel.channelValidator.Evaluate(self)) return true;
        if (target == null) return true;

        if (Vector3.Distance(self.transform.position, target.transform.position) <= Activatable.activationRange)
        {
            self.control.agentDestination = self.transform.position;
            self.LookAt(target.transform.position);
            target.StartActivate(self);
            return true;
        }
        else
        {
            if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
            if (self.control.IsMoveProhibitedByChannel()) return false;
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
        if (self.control.IsMoveProhibitedByChannel()) return false;
        self.control.agentDestination = destination;
        if (self.control.agent.enabled && self.control.agent.path != null && self.control.agent.path.corners.Length > 1)
        {
            self.LookAt(self.control.agent.path.corners[1]);
        }
        if (Vector3.Distance(destination, self.transform.position) < 0.5f && self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessAttack(LivingThing target)
    {
        if (self.control.skillSet[0] == null) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) > self.control.skillSet[0].range) return true;
        if (!self.control.skillSet[0].selfValidator.Evaluate(self)) return true;
        if (!self.control.skillSet[0].targetValidator.Evaluate(self, target)) return true;
        if (!self.control.skillSet[0].isCooledDown) return true;
        if (!self.control.skillSet[0].IsReady()) return true;
        if (self.control.IsAttackProhibitedByChannel()) return true;
        if (target.IsDead()) return true;
        self.LookAt(target.transform.position);
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

        if(lastAttackMoveCheckTime < 0 || Time.time - lastAttackMoveCheckTime >= 1f / self.control.attackMoveTargetChecksForSecond)
        {
            lastAttackMoveCheckTime = Time.time;
            List<LivingThing> targets = self.GetAllTargetsInRange(self.transform.position, self.control.skillSet[0].range, self.control.skillSet[0].targetValidator);
            if(targets.Count == 0)
            {
                if (self.control.IsMoveProhibitedByChannel()) return false;
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

    private bool ProcessChase(LivingThing target)
    {

        if (SelfValidator.CancelsChaseCommand.Evaluate(self)) return true;
        if (target == null || target.IsDead()) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) <= self.control.skillSet[0].range)
        {

            self.control.agentDestination = self.transform.position;
            ProcessAttack(target);
        }
        else
        {

            if (self.control.IsMoveProhibitedByChannel()) return false;

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
        if (self.control.IsAbilityProhibitedByChannel()) return false;
        if (!trigger.isCooledDown) return true;
        if (!trigger.selfValidator.Evaluate(self)) return true;
        if (!self.HasMana(trigger.manaCost)) return true;
        if (!trigger.IsReady()) return true;
        switch (trigger.targetingType)
        {
            case AbilityTrigger.TargetingType.None:
                self.control.agentDestination = self.transform.position;
                trigger.Cast(info);
                return true;
            case AbilityTrigger.TargetingType.Target:
                if (!trigger.targetValidator.Evaluate(self, info.target)) return true;
                if (Vector3.Distance(self.transform.position, info.target.transform.position) > trigger.range)
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
                    trigger.Cast(info);
                    return true;
                }
            case AbilityTrigger.TargetingType.Direction:
                self.LookAt(self.transform.position + info.directionVector);
                trigger.Cast(info);
                return true;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(self.transform.position, info.point) > trigger.range)
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
                    trigger.Cast(info);
                    return true;
                }
            case AbilityTrigger.TargetingType.PointNonStrict:
                if(Vector3.Distance(self.transform.position, info.point) > trigger.range)
                {
                    info.point = self.transform.position + (info.point - self.transform.position).normalized * trigger.range;
                }
                self.LookAt(info.point);
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
    public float[] cooldownTime = new float[6];

    [Header("AI Settings")]
    public AIMode mode = AIMode.None;

    public float aiCheckInterval = 0.5f;
    public float autoChaseRange = 4f;
    public float autoChaseOutOfRangeCancelTime = 2f;
    [Header("AttackMove Settings")]
    public float attackMoveTargetChecksForSecond = 4f;
    [Header("Misc. Settings")]
    public float angularSpeed = 600f;

    [HideInInspector]
    public List<Command> reservedCommands = new List<Command>();
    private float lastAICheckTime = 0f;

    private Animator animator;
    private LivingThing livingThing;

    



    private Command currentCommand { get { return reservedCommands.Count == 0 ? null : reservedCommands[0]; } }
    private List<Channel> ongoingChannels = new List<Channel>();
    private Quaternion desiredRotation = Quaternion.identity;



    [HideInInspector]
    public Vector3 agentDestination;

    #region Commands For Local
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

    public void CommandActivate(Activatable target, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Activate, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
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

    public void CommandConsumable(Consumable consumable, CastInfo info, bool reserve = false)
    {
        Command command = new Command(livingThing, CommandType.Consumable, consumable, info);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }



    #endregion Commands For Local


    #region Functions For Local

    public void LookAt(Vector3 location, bool immediately = false)
    {
        Vector3 euler = Quaternion.LookRotation(location - transform.position, transform.up).eulerAngles;
        euler.x = 0;
        euler.z = 0;
        desiredRotation = Quaternion.Euler(euler);
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
    
    public bool IsMoveProhibitedByChannel()
    {
        bool result = false;
        for(int i = 0; i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded()) continue;
            if (!ongoingChannels[i].canMove)
            {
                if (ongoingChannels[i].canBeCanceledByCaster)
                {
                    ongoingChannels[i].Cancel();
                }
                else
                {
                    result = true;
                }
            }
        }
        return result;
    }


    public bool IsAttackProhibitedByChannel()
    {
        bool result = false;
        for (int i = 0; i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded()) continue;
            if (!ongoingChannels[i].canAttack)
            {
                if (ongoingChannels[i].canBeCanceledByCaster)
                {
                    ongoingChannels[i].Cancel();
                }
                else
                {
                    result = true;
                }
            }
        }
        return result;
    }

    public bool IsAbilityProhibitedByChannel()
    {
        bool result = false;
        for (int i = 0; i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded()) continue;
            if (!ongoingChannels[i].canUseAbility)
            {
                if (ongoingChannels[i].canBeCanceledByCaster)
                {
                    ongoingChannels[i].Cancel();
                }
                else
                {
                    result = true;
                }
            }
        }
        return result;
    }

    #endregion Functions For Local
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        agent = GetComponent<NavMeshAgent>();
        animator = transform.Find("Model").GetComponent<Animator>();
        agent.updateRotation = false;
        agentDestination = transform.position;
        if (agent.enabled && !agent.isOnNavMesh) FixPosition();
    }


    private void Update()
    {
        if (!photonView.IsMine) return;


        cooldownTime[0] = Mathf.MoveTowards(cooldownTime[0], 0, Time.deltaTime);
        for(int i = 1; i < cooldownTime.Length; i++)
        {
            cooldownTime[i] = Mathf.MoveTowards(cooldownTime[i], 0, Time.deltaTime * (1f + (livingThing.stat.finalCooldownReduction / 100)));
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, angularSpeed * Time.deltaTime);
        if (livingThing.statusEffect.IsAffectedBy(StatusEffectType.Airborne) ||
            livingThing.statusEffect.IsAffectedBy(StatusEffectType.Dash) ||
            livingThing.statusEffect.IsAffectedBy(StatusEffectType.Stasis))
        {
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
        }

        agent.speed = livingThing.stat.finalMovementSpeed / 100f * (100f + livingThing.statusEffect.totalSpeedAmount) / 100f * (100f - livingThing.statusEffect.totalSlowAmount) / 100f;

        List<Channel> channelsToRemove = new List<Channel>();


        for(int i = 0;i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded())
            {
                channelsToRemove.Add(ongoingChannels[i]);
            }
            else
            {
                if (ongoingChannels[i].channelValidator.Evaluate(livingThing))
                {
                    ongoingChannels[i].Tick();
                }
                else
                {
                    ongoingChannels[i].Cancel();
                    channelsToRemove.Add(ongoingChannels[i]);
                }
                
            }
        }

        foreach(Channel channel in channelsToRemove)
        {
            ongoingChannels.Remove(channel);
        }


        if(currentCommand == null)
        {
            if (Time.time - lastAICheckTime >= aiCheckInterval)
            {
                lastAICheckTime = Time.time;
                switch (mode)
                {
                    case AIMode.None:
                        break;
                    case AIMode.AutoAttackInRange:
                        if (skillSet[0] == null) break;
                        if (!skillSet[0].selfValidator.Evaluate(livingThing)) break;
                        List<LivingThing> aaTargets = livingThing.GetAllTargetsInRange(transform.position, skillSet[0].range, skillSet[0].targetValidator);
                        for(int i = 0; i < aaTargets.Count; i++)
                        {
                            if (!aaTargets[i].IsDead() && skillSet[0].targetValidator.Evaluate(livingThing, aaTargets[i]))
                            {
                                CommandAttack(aaTargets[i]);
                                break;
                            }
                        }
                        break;
                    case AIMode.AutoChaseToAttack:
                        if (skillSet[0] == null) break;
                        if (!skillSet[0].selfValidator.Evaluate(livingThing)) break;
                        List<LivingThing> acTargets = livingThing.GetAllTargetsInRange(transform.position, autoChaseRange, skillSet[0].targetValidator);
                        for (int i = 0; i < acTargets.Count; i++)
                        {
                            if (!acTargets[i].IsDead() && skillSet[0].targetValidator.Evaluate(livingThing, acTargets[i]))
                            {
                                CommandAutoChase(acTargets[i]);
                                break;
                            }
                        }
                        break;
                }
            }
        }

        if (currentCommand != null)
        {
            bool isCommandFinished = currentCommand.Process();
            if (isCommandFinished) reservedCommands.RemoveAt(0);
        }
        else
        {
            agentDestination = transform.position;
        }

        if (agent.enabled && agent.isOnNavMesh) agent.destination = agentDestination;

        if(agent.enabled && !agent.isOnNavMesh) FixPosition();
 


        WalkCheck();
    }

    private void FixPosition()
    {
        RaycastHit info;
        if (Physics.Raycast(transform.position, Vector3.down, out info, LayerMask.GetMask("Ground")))
        {
            agent.enabled = false;
            transform.position = info.point;
            agent.enabled = true;

        }
    }

    private bool wasWalking = false;
    private void WalkCheck()
    {
        if(!wasWalking && agent.enabled && agent.desiredVelocity.magnitude > float.Epsilon)
        {
            photonView.RPC("RpcStartWalking", RpcTarget.All, agent.destination);
            wasWalking = true;
        } else if (wasWalking && (!agent.enabled || agent.desiredVelocity.magnitude < float.Epsilon))
        {
            photonView.RPC("RpcStopWalking", RpcTarget.All);
            wasWalking = false;
        }
    }

    [PunRPC]
    private void RpcStartWalking(Vector3 destination)
    {
        animator.SetBool("IsWalking", true);
        livingThing.OnStartWalking.Invoke(new InfoStartWalking() { livingThing = this.livingThing, destination = destination });
    }

    [PunRPC]
    private void RpcStopWalking()
    {
        animator.SetBool("IsWalking", false);
        livingThing.OnStopWalking.Invoke(new InfoStopWalking() { livingThing = livingThing });
    }


}
