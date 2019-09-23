using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LivingThingControl : MonoBehaviour
{
    private LivingThing livingThing;
    private NavMeshAgent navMeshAgent;

    private enum ActionType { None, Move, AttackMove, Spell, Channeling }
    [SerializeField]
    private ActionType reservedAction = ActionType.None;
    private SpellTrigger actionSpellTrigger;
    private SpellManager.CastInfo actionInfo;

    [SerializeField]
    private System.Action channelSuccessCallback;
    [SerializeField]
    private System.Action channelCanceledCallback;
    [SerializeField]
    private float channelRemainingTime;
    [SerializeField]
    private bool channelIsCanceledByMoveCommand;

    public SpellTrigger basicAttackSpellTrigger;

    public SpellTrigger[] keybindings = new SpellTrigger[4];
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
        livingThing = GetComponent<LivingThing>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        actionInfo.owner = GetComponent<LivingThing>();
    }

    private Vector3 Flat(Vector3 vector)
    {
        Vector3 temp = vector;
        vector.y = 0;
        return temp;
    }

    public void StartMove(Vector3 location)
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

    private void Update()
    {
        switch (reservedAction)
        {
            case ActionType.None:
                navMeshAgent.SetDestination(transform.position);
                break;
            case ActionType.Move:
                if (navMeshAgent.isStopped)
                {
                    reservedAction = ActionType.None;
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
