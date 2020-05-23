using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.Events;
using System;

public enum CommandType { Move, Attack, AttackMove, Chase, AutoChase, Ability, Activate, Consumable, AutoAttackInRange }

public enum AIMode { None, AutoAttackInRange, AutoChaseToAttack }



[System.Serializable]
public class Channel
{
    public SelfValidator channelValidator;
    public Entity owner;
    public float duration;

    public bool canMove;
    public bool canAttack;
    public bool canUseAbility;

    public bool canBeCanceledByCaster;

    public bool isBasicAttack = false;

    public System.Action finishedCallback = null;
    public System.Action canceledCallback = null;

    

    private bool hasEnded = false;

    public Channel(SelfValidator channelValidator, float duration, bool canMove, bool canAttack, bool canUseAbility, bool canBeCanceledByCaster, System.Action finishedCallback = null, System.Action canceledCallback = null, bool isBasicAttack = false)
    {
        this.channelValidator = channelValidator;
        this.duration = duration;
        this.canMove = canMove;
        this.canAttack = canAttack;
        this.canUseAbility = canUseAbility;
        this.canBeCanceledByCaster = canBeCanceledByCaster;
        this.finishedCallback = finishedCallback;
        this.canceledCallback = canceledCallback;
        this.isBasicAttack = isBasicAttack;
    }

    public void Tick()
    {
        if (hasEnded) return;

        if(duration == 0)
        {
            hasEnded = true;
            if (finishedCallback != null) finishedCallback.Invoke();
        }

        if (isBasicAttack) duration = Mathf.MoveTowards(duration, 0, Time.deltaTime * (100f + owner.statusEffect.status.haste) / 100f);
        else duration = Mathf.MoveTowards(duration, 0, Time.deltaTime);
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

public class EntityControl : MonoBehaviourPun
{
    public NavMeshAgent agent { get; private set; }

    public AbilityTrigger[] skillSet = new AbilityTrigger[7];
    
    [NonSerialized]
    public float[] cooldownTime = new float[7];

    [Header("AI Settings")]
    public AIMode mode = AIMode.None;

    public float aiInterval = 0.5f;
    public float autoChaseRange = 4f;
    public float autoChaseOutOfRangeCancelTime = 2f;
    public bool autocastSpells = false;
    public bool startWithAbilitiesCoolingdown = false;
    public float spellCastChance = 0.5f;
    [Header("AttackMove Settings")]
    public float attackMoveTargetChecksForSecond = 4f;
    [Header("Misc. Settings")]
    public float angularSpeed = 600f;

    //[HideInInspector]
    public List<Command> reservedCommands = new List<Command>();
    private float lastAICheckTime = 0f;

    private Animator animator;
    private Entity entity;

    



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
        Command command = new Command(entity, CommandType.Activate, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandMove(Vector3 destination, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.Move, destination);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAttack(Entity target, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.Attack, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAttackMove(Vector3 destination, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.AttackMove, destination);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandChase(Entity target, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.Chase, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAutoChase(Entity target, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.AutoChase, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }
    public void CommandAutoAttackInRange(Entity target, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.AutoAttackInRange, target);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }

    public void CommandAbility(AbilityTrigger trigger, CastInfo info, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.Ability, trigger, info);
        Command temp = null;
        if (!trigger.dontCancelBasicCommands)
        {
            if (!reserve) reservedCommands.Clear();
            reservedCommands.Add(command);
        }
        else
        {
            if (!reserve && reservedCommands.Count != 0)
            {
                if (reservedCommands[0].type != CommandType.Ability && reservedCommands[0].type != CommandType.Consumable)
                {
                    temp = reservedCommands[0];
                }
                reservedCommands.Clear();
                reservedCommands.Add(command);
                if (temp != null) reservedCommands.Add(temp);
            }
            else
            {
                reservedCommands.Add(command);
            }
        }
    }

    public void CommandConsumable(Consumable consumable, CastInfo info, bool reserve = false)
    {
        Command command = new Command(entity, CommandType.Consumable, consumable, info);
        if (!reserve) reservedCommands.Clear();
        reservedCommands.Add(command);
    }



    #endregion Commands For Local


    #region Functions For Local

    public void LookAt(Vector3 location, bool immediately = false)
    {
        if (Vector3.Distance(transform.position, location) <= float.Epsilon) return;
        Vector3 euler = Quaternion.LookRotation(location - transform.position, transform.up).eulerAngles;
        euler.x = 0;
        euler.z = 0;
        desiredRotation = Quaternion.Euler(euler);
        if (immediately)
        {
            transform.rotation = desiredRotation;
        }
    }

    public void StartChanneling(Channel channel)
    {
        channel.owner = entity;
        if (channel.isBasicAttack)
        {
            channel.duration *= 1f / entity.stat.finalAttacksPerSecond;
        }
        ongoingChannels.Add(channel);
    }
    
    public bool IsMoveProhibitedByChannel(bool cancelCancelableChannels)
    {
        for(int i = 0; i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded()) continue;
            if (!ongoingChannels[i].canMove)
            {
                if (ongoingChannels[i].canBeCanceledByCaster && cancelCancelableChannels)
                {
                    ongoingChannels[i].Cancel();
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }


    public bool IsAttackProhibitedByChannel(bool cancelsCancelableChannels = true)
    {
        bool result = false;
        for (int i = 0; i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded()) continue;
            if (!ongoingChannels[i].canAttack)
            {
                if (ongoingChannels[i].canBeCanceledByCaster && cancelsCancelableChannels)
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
        entity = GetComponent<Entity>();
        agent = GetComponent<NavMeshAgent>();
        animator = transform.Find("Model").GetComponent<Animator>();
        agent.updateRotation = false;
        agentDestination = transform.position;
        if (agent.enabled && !agent.isOnNavMesh) FixPosition();
    }

    private void Start()
    {
        if (startWithAbilitiesCoolingdown)
        {
            for(int i = 0; i < cooldownTime.Length; i++)
            {
                if(skillSet.Length > i && skillSet[i] != null)
                {
                    cooldownTime[i] = skillSet[i].cooldownTime;
                }
            }
        }
    }

    private void Update()
    {
        bool canTick = SelfValidator.CanTick.Evaluate(entity);
        if ((entity.ongoingDisplacement != null && entity.ongoingDisplacement.type == Displacement.DisplacementType.TowardsTarget) ||
            entity.statusEffect.IsAffectedBy(StatusEffectType.Stasis) ||
            entity.ongoingDisplacement != null && entity.ongoingDisplacement.type == Displacement.DisplacementType.ByVector && entity.ongoingDisplacement.ignoreCollision)
        {
            agent.enabled = false;
        }
        else agent.enabled = true;


        if (!photonView.IsMine) return;

        if (canTick)
        {
            cooldownTime[0] = Mathf.MoveTowards(cooldownTime[0], 0, Time.deltaTime * (1f + (entity.statusEffect.status.haste / 100f)));
            for (int i = 1; i < cooldownTime.Length; i++)
            {
                cooldownTime[i] = Mathf.MoveTowards(cooldownTime[i], 0, Time.deltaTime * (1f + (entity.stat.finalCooldownReduction / 100f)));
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, angularSpeed * Time.deltaTime);
        }

        if (agent.enabled) Debug.DrawLine(transform.position, agent.destination, Color.green);


        bool canWalk = !SelfValidator.CancelsMoveCommand.Evaluate(entity);
        if (!canWalk)
        {
            agent.speed = 0f;

        }
        else
        {
            agent.speed = entity.stat.finalMovementSpeed / 100f * (100f + entity.statusEffect.status.speed) / 100f * (100f - entity.statusEffect.status.slow) / 100f;
        }




        if (!canTick)
        {
            return;
        }

        List<Channel> channelsToRemove = new List<Channel>();
        for (int i = 0;i < ongoingChannels.Count; i++)
        {
            if (ongoingChannels[i].HasEnded())
            {
                channelsToRemove.Add(ongoingChannels[i]);
            }
            else
            {
                if (ongoingChannels[i].channelValidator.Evaluate(entity))
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


        if (Time.time - lastAICheckTime >= aiInterval)
        { 
            if (autocastSpells && (currentCommand == null || currentCommand.type != CommandType.Ability) && UnityEngine.Random.value < spellCastChance)
            {
                for(int i = 1; i < skillSet.Length; i++)
                {
                    if (cooldownTime[i] > 0) continue;
                    if (skillSet[i] == null) continue;
                    if (!skillSet[i].selfValidator.Evaluate(entity)) continue;
                    if (!entity.HasMana(skillSet[i].manaCost)) continue;
                    if (!skillSet[i].IsReady()) continue;
                    if (IsAbilityProhibitedByChannel()) continue;
                    if (skillSet[i].targetingType == AbilityTrigger.TargetingType.Target)
                    {
                        List<Entity> targets = entity.GetAllTargetsInRange(transform.position, skillSet[i].range, skillSet[i].targetValidator);
                        if(targets.Count != 0)
                        {
                            CommandAbility(skillSet[i], new CastInfo { target = targets[0], owner = entity, point = targets[0].transform.position, directionVector = (targets[0].transform.position - transform.position).normalized });
                            break;
                        }
                    }
                    else if (skillSet[i].targetingType == AbilityTrigger.TargetingType.None)
                    {
                        CommandAbility(skillSet[i], new CastInfo { owner = entity });
                        break;
                    }
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

        if (Time.time - lastAICheckTime >= aiInterval)
        {
            lastAICheckTime = Time.time;
            List<Entity> acTargets;

            if (currentCommand == null)
            {

                switch (mode)
                {
                    case AIMode.None:
                        break;
                    case AIMode.AutoAttackInRange:
                        if (skillSet[0] == null) break;
                        if (!skillSet[0].selfValidator.Evaluate(entity)) break;
                        acTargets = entity.GetAllTargetsInRange(transform.position, skillSet[0].range, skillSet[0].targetValidator);
                        for (int i = 0; i < acTargets.Count; i++)
                        {
                            if (!acTargets[i].IsDead() && skillSet[0].targetValidator.Evaluate(entity, acTargets[i]))
                            {
                                CommandAutoAttackInRange(acTargets[i]);
                                break;
                            }
                        }
                        break;
                    /*
                    if (skillSet[0] == null) break;
                    if (!skillSet[0].selfValidator.Evaluate(livingThing)) break;
                    List<LivingThing> aaTargets = livingThing.GetAllTargetsInRange(transform.position, skillSet[0].range, skillSet[0].targetValidator);
                    for (int i = 0; i < aaTargets.Count; i++)
                    {
                        if (!aaTargets[i].IsDead() && skillSet[0].targetValidator.Evaluate(livingThing, aaTargets[i]))
                        {
                            CommandAttack(aaTargets[i]);
                            break;
                        }
                    }
                    break;
                    */
                    case AIMode.AutoChaseToAttack:
                        if (skillSet[0] == null) break;
                        if (!skillSet[0].selfValidator.Evaluate(entity)) break;
                        acTargets = entity.GetAllTargetsInRange(transform.position, autoChaseRange, skillSet[0].targetValidator);
                        for (int i = 0; i < acTargets.Count; i++)
                        {
                            if (!acTargets[i].IsDead() && skillSet[0].targetValidator.Evaluate(entity, acTargets[i]))
                            {
                                CommandAutoChase(acTargets[i]);
                                break;
                            }
                        }
                        break;
                }


            }



        }

        if (currentCommand == null) agentDestination = transform.position;

        if (agent.enabled && agent.isOnNavMesh) agent.destination = agentDestination;

        if(agent.enabled && agent.isOnNavMesh)
        {
            if (agent.destination == transform.position)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }
        }


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
        bool isMoveProhibited = IsMoveProhibitedByChannel(false);
        bool isMoveCommandCanceled = SelfValidator.CancelsMoveCommand.Evaluate(entity);
        if (!wasWalking && agent.enabled && agent.desiredVelocity.magnitude > float.Epsilon && !isMoveProhibited && !isMoveCommandCanceled)
        {
            photonView.RPC(nameof(RpcStartWalking), RpcTarget.All, agent.destination);
            wasWalking = true;
        }
        else if (wasWalking && (!agent.enabled || agent.desiredVelocity.magnitude < float.Epsilon || isMoveProhibited || isMoveCommandCanceled))
        {
            photonView.RPC(nameof(RpcStopWalking), RpcTarget.All);
            wasWalking = false;
        }
    }

    [PunRPC]
    private void RpcStartWalking(Vector3 destination)
    {
        animator.SetBool("IsWalking", true);
    }

    [PunRPC]
    private void RpcStopWalking()
    {
        animator.SetBool("IsWalking", false);
    }


}
