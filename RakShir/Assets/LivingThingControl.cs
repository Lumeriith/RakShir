using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
public class LivingThingControl : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private LivingThing livingThing;

    private Vector3 navMeshAgentDestination;
    private enum ActionType { None, Move, AttackMove, AbilityTrigger, Channel, ChannelMovable, SoftChannel }

    private ActionType reservedAction = ActionType.None;
    private AbilityTrigger actionAbilityTrigger;
    private AbilityInstanceManager.CastInfo actionInfo;

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

    public void StartChanneling(float channelTime, System.Action successCallback, System.Action canceledCallback, bool movable = false)
    {
        reservedAction = movable ? ActionType.ChannelMovable : ActionType.Channel;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = channelTime;
    }

    public void StartBasicAttackChanneling(float ratio, System.Action successCallback, System.Action canceledCallback)
    {
        reservedAction = ActionType.SoftChannel;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = ratio * (1 / livingThing.stat.finalAttacksPerSecond);
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
    }

    private Vector3 Flat(Vector3 vector)
    {
        Vector3 temp = vector;
        temp.y = 0;
        return temp;
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
        navMeshAgentDestination = location;
    }



    private void DoAggroCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(Flat(transform.position), aggroRange, maskLivingThing);
        Collider myCollider = GetComponent<Collider>();
        LivingThing closestTarget = null;
        LivingThing temp;
        float closestDistance = Mathf.Infinity;
        float distance;

        foreach(Collider collider in colliders)
        {
            distance = Vector3.Distance(Flat(collider.transform.position), Flat(transform.position));
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

        
        navMeshAgent.enabled = SelfValidator.CanHaveNavMeshEnabled.Evaluate(livingThing);

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

                if (Vector3.Distance(Flat(navMeshAgentDestination), Flat(transform.position)) < 0.1f)
                {
                    reservedAction = ActionType.None;
                }
                break;
            case ActionType.AttackMove:
                if (navMeshAgent.isStopped)
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
    public void ReserveAbilityTrigger(AbilityTrigger abilityTrigger, Vector3 point, Vector3 directionVector, LivingThing target)
    {
        bool skipValidationCheck = false;
        if (!skipValidationCheck)
        {
            if (!abilityTrigger.selfValidator.Evaluate(livingThing)) return;
            if (abilityTrigger.targetingType == AbilityTrigger.TargetingType.Target && !abilityTrigger.targetValidator.Evaluate(livingThing, target)) return;
        }
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
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.Target:
                
                Vector3 targetPosition = Flat(actionInfo.target.transform.position);

                if (Vector3.Distance(Flat(transform.position), targetPosition) <= actionAbilityTrigger.range)
                {
                    navMeshAgentDestination = transform.position;
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionAbilityTrigger.OnCast(actionInfo);
                    
                }
                else
                {
                    navMeshAgentDestination = targetPosition;
                }
                break;
            case AbilityTrigger.TargetingType.PointNonStrict:
                navMeshAgentDestination = transform.position;
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(Flat(transform.position), Flat(actionInfo.point)) <= actionAbilityTrigger.range)
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
                break;
        }
    }
}
