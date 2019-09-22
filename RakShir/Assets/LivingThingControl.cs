using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class LivingThingControl : MonoBehaviour
{
    private LivingThing livingThing;
    private NavMeshAgent navMeshAgent;

    private enum ActionType { None, Move, AttackMove, Spell }

    private ActionType reservedAction = ActionType.None;
    private SpellTrigger actionSpellTrigger;
    private SpellTrigger.CastInfo actionInfo;

    public SpellTrigger basicAttackSpellTrigger;

    public SpellTrigger[] keybindings = new SpellTrigger[4];

    void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private Vector3 Flat(Vector3 vector)
    {
        Vector3 temp = vector;
        vector.y = 0;
        return temp;
    }

    public void StartMove(Vector3 location)
    {
        reservedAction = ActionType.Move;
        navMeshAgent.SetDestination(location);
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
        }
    }

    public void ReserveSpellTrigger(SpellTrigger spellTrigger, Vector3 point, Vector3 directionVector, Collider target)
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
                actionSpellTrigger.OnCast(actionInfo);
                reservedAction = ActionType.None;
                break;
            case SpellTrigger.TargetingType.Direction:
                actionSpellTrigger.OnCast(actionInfo);
                reservedAction = ActionType.None;
                break;
            case SpellTrigger.TargetingType.Target:
                Vector3 targetPosition = Flat(actionInfo.target.transform.position);

                if (Vector3.Distance(Flat(transform.position), targetPosition) <= actionSpellTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    actionSpellTrigger.OnCast(actionInfo);
                    reservedAction = ActionType.None;
                }
                else
                {
                    navMeshAgent.SetDestination(targetPosition);
                }
                break;
            case SpellTrigger.TargetingType.PointNonStrict:
                navMeshAgent.SetDestination(actionInfo.target.transform.position);
                break;
            case SpellTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(Flat(transform.position), Flat(actionInfo.point)) <= actionSpellTrigger.range)
                {
                    navMeshAgent.SetDestination(transform.position);
                    actionSpellTrigger.OnCast(actionInfo);
                    reservedAction = ActionType.None;
                }
                else
                {
                    navMeshAgent.SetDestination(actionInfo.point);
                }
                break;
        }
    }
}
