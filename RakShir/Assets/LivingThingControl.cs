using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
public class LivingThingControl : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private LivingThing livingThing;

    private enum ActionType { None, Move, AttackMove, Ability, Channeling } // START HERE. REMOVE CHANNELING OR THINK STRAIGHT AND DO SOMETHING GOOD.
    [SerializeField]
    private ActionType reservedAction = ActionType.None;
    private AbilityTrigger actionAbilityTrigger;
    private AbilityInstanceManager.CastInfo actionInfo;

    private System.Action channelSuccessCallback;
    private System.Action channelCanceledCallback;
    private float channelRemainingTime;
    private bool channelIsCanceledByMoveCommand;

    public AbilityTrigger basicAttackAbilityTrigger;

    public AbilityTrigger[] keybindings = new AbilityTrigger[4];

    [Header("Aggro Settings")]
    public bool aggroAutomatically;
    public float aggroRange;
    public float deaggroRange;
    public float aggroChecksPerSecond;

    private float lastAggroCheckTime;

    public void StartChanneling(float channelTime, System.Action successCallback, System.Action canceledCallback, bool isCanceledByMoveCommand = false)
    {
        reservedAction = ActionType.Channeling;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = channelTime;
        channelIsCanceledByMoveCommand = isCanceledByMoveCommand;
    }

    public void StartBasicAttackChanneling(float ratio, System.Action successCallback, System.Action canceledCallback)
    {
        reservedAction = ActionType.Channeling;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = ratio * (1 / livingThing.stat.finalAttacksPerSecond);
        channelIsCanceledByMoveCommand = true;
    }

    public void CancelChanneling()
    {
        if(reservedAction == ActionType.Channeling)
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
        if (reservedAction == ActionType.Channeling)
        {
            if (channelIsCanceledByMoveCommand)
            {
                reservedAction = ActionType.Move;
                navMeshAgent.SetDestination(location);
                channelCanceledCallback.Invoke();
            }
            else
            {
                return;
            }
        }
        else
        {
            reservedAction = ActionType.Move;
            navMeshAgent.SetDestination(location);
        }
    }

    public void StartAttackMoving(Vector3 location)
    {
        if (reservedAction == ActionType.Channeling)
        {
            if (channelIsCanceledByMoveCommand)
            {
                reservedAction = ActionType.AttackMove;
                navMeshAgent.SetDestination(location);
                channelCanceledCallback.Invoke();
            }
            else
            {
                return;
            }
        }
        else
        {
            reservedAction = ActionType.AttackMove;
            navMeshAgent.SetDestination(location);
        }
    }



    private void DoAggroCheck()
    {
        Collider[] colliders = Physics.OverlapSphere(Flat(transform.position), aggroRange, basicAttackAbilityTrigger.targetMask);
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
                if (temp != null)
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
        switch (reservedAction)
        {
            case ActionType.None:
                navMeshAgent.SetDestination(transform.position);
                if(aggroAutomatically && (Time.time - lastAggroCheckTime > 1 / aggroChecksPerSecond))
                {
                    lastAggroCheckTime = Time.time;
                    DoAggroCheck();
                }
                break;
            case ActionType.Move:

                if (Vector3.Distance(Flat(navMeshAgent.destination), Flat(transform.position)) < 0.1f)
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
            case ActionType.Ability:
                TryCastReservedAbilityInstance();
                break;
            case ActionType.Channeling:
                channelRemainingTime = Mathf.MoveTowards(channelRemainingTime, 0, Time.deltaTime);
                if(channelRemainingTime == 0)
                {
                    reservedAction = ActionType.None;
                    channelSuccessCallback.Invoke();
                }
                break;
        }


    }

    public void ReserveAbilityTrigger(AbilityTrigger abilityTrigger, Vector3 point, Vector3 directionVector, LivingThing target)
    {
        reservedAction = ActionType.Ability;
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
                // TODO: Check if the target is valid, if invalid cancel the action
                Vector3 targetPosition = Flat(actionInfo.target.transform.position);

                if (Vector3.Distance(Flat(transform.position), targetPosition) <= actionAbilityTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionAbilityTrigger.OnCast(actionInfo);
                    
                }
                else
                {
                    navMeshAgent.SetDestination(targetPosition);
                }
                break;
            case AbilityTrigger.TargetingType.PointNonStrict:
                navMeshAgent.SetDestination(transform.position);
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionAbilityTrigger.OnCast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(Flat(transform.position), Flat(actionInfo.point)) <= actionAbilityTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionAbilityTrigger.OnCast(actionInfo);

                }
                else
                {
                    navMeshAgent.SetDestination(actionInfo.point);
                }
                break;
        }
    }
}
