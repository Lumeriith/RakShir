using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Photon.Pun;
public class LivingThingControl : MonoBehaviourPun
{
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    private LivingThing livingThing;

    private Vector3 navMeshAgentDestination;
    private enum ActionType { None, Move, AttackMove, AbilityTrigger, Channel, ChannelMovable, SoftChannel }

    private ActionType reservedAction = ActionType.None;
    private AbilityTrigger actionAbilityTrigger;
    private CastInfo actionInfo;

    private System.Action channelSuccessCallback;
    private System.Action channelCanceledCallback;
    private float channelRemainingTime;



    public AbilityTrigger basicAttackAbilityTrigger;

    public AbilityTrigger[] keybindings = new AbilityTrigger[4];


    [Header("Aggro Settings")]
    public bool aggroAutomatically;
    public float aggroRange;
    public float deaggroRange;
    public float aggroChecksPerSecond;

    private float lastAggroCheckTime;

    [Header("Preconfigurations")]
    public LayerMask maskLivingThing;

    private const float modelTurnSpeed = 800;
    public Quaternion desiredRotation { get; private set; }

    public void StartChanneling(float channelTime, System.Action successCallback = null, System.Action canceledCallback = null, bool movable = false)
    {
        reservedAction = movable ? ActionType.ChannelMovable : ActionType.Channel;
        channelSuccessCallback = successCallback != null ? successCallback : () => { };
        channelCanceledCallback = canceledCallback != null ? canceledCallback : () => { };
        channelRemainingTime = channelTime;
    }

    public void StartBasicAttackChanneling(float ratio, System.Action successCallback, System.Action canceledCallback)
    {
        reservedAction = ActionType.SoftChannel;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = ratio * (1 / livingThing.stat.finalAttacksPerSecond);
    }

    private void DoRotateTick()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, modelTurnSpeed * Time.deltaTime);
    }

    private void LookAt(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        desiredRotation = Quaternion.Euler(euler);
    }

    private void LookAt(Vector3 lookLocation)
    {
        if ((lookLocation - transform.position).magnitude <= float.Epsilon) return;
        LookAt(Quaternion.LookRotation(lookLocation - transform.position, Vector3.up));
    }

    public void CancelChanneling()
    {
        if (reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable || reservedAction == ActionType.SoftChannel)
        {
            reservedAction = ActionType.None;
            channelCanceledCallback.Invoke();
        }
    }


    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        actionInfo.owner = GetComponent<LivingThing>();
        livingThing = GetComponent<LivingThing>();
        navMeshAgent.updateRotation = false;
    }

    public void StartMoving(Vector3 location)
    {
        if(reservedAction == ActionType.SoftChannel)
        {
            CancelChanneling();
        }
        else if (reservedAction == ActionType.Channel)
        {
            return;
        }

        reservedAction = ActionType.Move;
        navMeshAgentDestination = location;
    }

    public void StartAttackMoving(Vector3 location)
    {
        if (reservedAction == ActionType.SoftChannel)
        {
            CancelChanneling();
        }
        else if (reservedAction == ActionType.Channel)
        {
            return;
        }

        reservedAction = ActionType.AttackMove;
        lastAggroCheckTime -= 1/aggroChecksPerSecond;
        navMeshAgentDestination = location;
    }



    private void DoAggroCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, aggroRange, maskLivingThing);
        Collider myCollider = GetComponent<Collider>();
        LivingThing closestTarget = null;
        LivingThing temp;
        float closestDistance = Mathf.Infinity;
        float distance;

        foreach(Collider collider in colliders)
        {
            distance = Vector3.Distance(collider.transform.position, transform.position);
            if (distance < closestDistance && closestTarget != myCollider)
            {
                temp = collider.GetComponent<LivingThing>();
                if (temp != null && basicAttackAbilityTrigger.targetValidator.Evaluate(livingThing, temp))
                {
                    closestTarget = temp;
                    closestDistance = distance;
                }

            }
        }

        if(closestTarget != null)
        {
            ReserveAbilityTrigger(basicAttackAbilityTrigger, Vector3.zero, Vector3.zero, closestTarget);
        }
    }

    private void Update()
    {
        navMeshAgent.enabled = SelfValidator.CanHaveNavMeshEnabled.Evaluate(livingThing);
        if (!photonView.IsMine) return;
        bool canTick = SelfValidator.CanTick.Evaluate(livingThing);

        if (reservedAction == ActionType.AbilityTrigger && !actionAbilityTrigger.CanActivate())
        {
            reservedAction = ActionType.None;
        }

        if((reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable || reservedAction == ActionType.SoftChannel) && !actionAbilityTrigger.CanActivate())
        {
            CancelChanneling();
        }

        if (SelfValidator.CanHaveMoveSpeedOverZero.Evaluate(livingThing))
        {
            navMeshAgent.speed = livingThing.stat.finalMovementSpeed / 100;
        }
        else
        {
            navMeshAgent.speed = 0;
        }

        if (reservedAction == ActionType.Move && !SelfValidator.CanHaveMoveActionReserved.Evaluate(livingThing))
        {
            reservedAction = ActionType.None;
        }


        DoRotateTick();

        switch (reservedAction)
        {
            case ActionType.None:
                navMeshAgentDestination = transform.position;
                if(aggroAutomatically && (Time.time - lastAggroCheckTime > 1 / aggroChecksPerSecond))
                {
                    lastAggroCheckTime = Time.time;
                    DoAggroCheck();
                }
                break;
            case ActionType.Move:
                LookAt(navMeshAgentDestination);
                
                if (Vector3.Distance(navMeshAgentDestination, transform.position) < 0.1f)
                {
                    reservedAction = ActionType.None;
                }
                break;
            case ActionType.AttackMove:
                LookAt(navMeshAgentDestination);
                if (navMeshAgent.enabled && navMeshAgent.isStopped)
                {
                    reservedAction = ActionType.None;
                }
                if (Time.time - lastAggroCheckTime > 1 / aggroChecksPerSecond)
                {
                    lastAggroCheckTime = Time.time;
                    DoAggroCheck();
                }

                break;
            case ActionType.AbilityTrigger:
                TryCastReservedAbilityInstance();
                break;
            case ActionType.Channel:
                HandleChannelTick();
                break;
            case ActionType.ChannelMovable:
                HandleChannelTick();
                break;
            case ActionType.SoftChannel:
                HandleChannelTick();
                break;
        }

        if (navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(navMeshAgentDestination);
        }
    }

    private void HandleChannelTick()
    {
        channelRemainingTime = Mathf.MoveTowards(channelRemainingTime, 0, Time.deltaTime);
        if (channelRemainingTime <= 0)
        {
            reservedAction = ActionType.None;
            channelSuccessCallback.Invoke();
        }
    }
    public void ReserveAbilityTrigger(AbilityTrigger abilityTrigger, Vector3 point = new Vector3(), Vector3 directionVector = new Vector3(), LivingThing target = null)
    {
        bool skipValidationCheck = false;
        if (!skipValidationCheck)
        {
            if (!abilityTrigger.selfValidator.Evaluate(livingThing)) return;
            if (abilityTrigger.targetingType == AbilityTrigger.TargetingType.Target && !abilityTrigger.targetValidator.Evaluate(livingThing, target)) return;
        }

        if ((reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable) &&
            basicAttackAbilityTrigger == abilityTrigger && actionInfo.target == target) return;

        reservedAction = ActionType.AbilityTrigger;
        actionAbilityTrigger = abilityTrigger;

        actionInfo.point = point;
        actionInfo.directionVector = directionVector;
        actionInfo.target = target;
    }

    private void TryCastReservedAbilityInstance()
    {



        switch (actionAbilityTrigger.targetingType)
        {
            case AbilityTrigger.TargetingType.None:
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.Direction:
                if (!actionAbilityTrigger.isCooledDown) break;
                LookAt(transform.position + actionInfo.directionVector);
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.Target:
                
                Vector3 targetPosition = actionInfo.target.transform.position;
                
                if (Vector3.Distance(transform.position, targetPosition) <= actionAbilityTrigger.range)
                {
                    
                    navMeshAgentDestination = transform.position;
                    LookAt(targetPosition);
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionAbilityTrigger.OnCast(actionInfo);
                    
                }
                else
                {
                    navMeshAgentDestination = targetPosition;
                    LookAt(navMeshAgentDestination);
                }
                
                break;
            case AbilityTrigger.TargetingType.PointNonStrict:
                navMeshAgentDestination = transform.position;
                LookAt(navMeshAgentDestination);
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(transform.position, actionInfo.point) <= actionAbilityTrigger.range)
                {
                    navMeshAgentDestination = transform.position;
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionAbilityTrigger.OnCast(actionInfo);

                }
                else
                {
                    navMeshAgentDestination = actionInfo.point;
                }
                LookAt(navMeshAgentDestination);
                break;
        }
    }
}
