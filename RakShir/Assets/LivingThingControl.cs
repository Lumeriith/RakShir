using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
public class LivingThingControl : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    private enum ActionType { None, Move, AttackMove, Spell, Channeling }
    private ActionType reservedAction = ActionType.None;
    private SpellTrigger actionSpellTrigger;
    private SpellManager.CastInfo actionInfo;

    private System.Action channelSuccessCallback;
    private System.Action channelCanceledCallback;
    private float channelRemainingTime;
    private bool channelIsCanceledByMoveCommand;

    public SpellTrigger basicAttackSpellTrigger;

    public SpellTrigger[] keybindings = new SpellTrigger[4];

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

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        actionInfo.owner = GetComponent<LivingThing>();
    }

    private Vector3 Flat(Vector3 vector)
    {
        Vector3 temp = vector;
        vector.y = 0;
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
                channelCanceledCallback();
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
                channelCanceledCallback();
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
        Collider[] colliders = Physics.OverlapSphere(Flat(transform.position), aggroRange, basicAttackSpellTrigger.targetMask);

        Collider closestTarget = null;
        float closestDistance = Mathf.Infinity;
        float distance;

        foreach(Collider collider in colliders)
        {
            distance = Vector3.Distance(Flat(collider.transform.position), Flat(transform.position));
            if (distance < closestDistance)
            {
                closestTarget = collider;
                closestDistance = distance;
            }
        }

        if(closestTarget != null)
        {
            ReserveSpellTrigger(basicAttackSpellTrigger, Vector3.zero, Vector3.zero, closestTarget.GetComponent<LivingThing>());
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
                if (navMeshAgent.isStopped)
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
            case ActionType.Spell:
                TryCastReservedSpell();
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

    public void ReserveSpellTrigger(SpellTrigger spellTrigger, Vector3 point, Vector3 directionVector, LivingThing target)
    {
        reservedAction = ActionType.Spell;
        actionSpellTrigger = spellTrigger;

        actionInfo.point = point;
        actionInfo.directionVector = directionVector;
        actionInfo.target = target;
    }

    private void TryCastReservedSpell()
    {
        switch (actionSpellTrigger.targetingType)
        {
            case SpellTrigger.TargetingType.None:
                if (!actionSpellTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionSpellTrigger.OnCast(actionInfo);
                break;
            case SpellTrigger.TargetingType.Direction:
                if (!actionSpellTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionSpellTrigger.OnCast(actionInfo);
                break;
            case SpellTrigger.TargetingType.Target:
                Vector3 targetPosition = Flat(actionInfo.target.transform.position);

                if (Vector3.Distance(Flat(transform.position), targetPosition) <= actionSpellTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    if (!actionSpellTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionSpellTrigger.OnCast(actionInfo);
                    
                }
                else
                {
                    navMeshAgent.SetDestination(targetPosition);
                }
                break;
            case SpellTrigger.TargetingType.PointNonStrict:
                navMeshAgent.SetDestination(transform.position);
                if (!actionSpellTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                actionSpellTrigger.OnCast(actionInfo);
                break;
            case SpellTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(Flat(transform.position), Flat(actionInfo.point)) <= actionSpellTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    if (!actionSpellTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    actionSpellTrigger.OnCast(actionInfo);

                }
                else
                {
                    navMeshAgent.SetDestination(actionInfo.point);
                }
                break;
        }
    }
}
