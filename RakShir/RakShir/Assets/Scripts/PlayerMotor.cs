using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMotor : MonoBehaviour
{

    [Header("Basic Attack Properties")]
    public float basicAttackReadyTime = 0.1f;
    public float basicAttackCooldown = 0.7f;
    public float basicAttackActivateRange = 1;
    public float basicAttackMaxRange = 1.5f;

    [Header("General")]
    public CommandType currentCommand = CommandType.None;
    public Vector3 moveCommandWorldPosition = Vector3.zero;

    private GameObject followTarget;





    private bool isReadyingBasicAttack = false;
    private float lastBasicAttackTime = 0;
    private float basicAttackReadyStartTime = 0;
    public enum CommandType { None, Move, Attack, Spell }
    private NavMeshAgent m_nma;
    void Start()
    {

        m_nma = GetComponent<NavMeshAgent>();
    }


    public void IssueStopCommand()
    {
        currentCommand = CommandType.None;
        m_nma.destination = transform.position;
        if (isReadyingBasicAttack)
        {
            isReadyingBasicAttack = false;
            CancelReadyingBasicAttack();
        }
    }

    public void IssueMoveCommand(Vector3 destination)
    {
        currentCommand = CommandType.Move;
        m_nma.destination = destination;
        if (isReadyingBasicAttack)
        {
            isReadyingBasicAttack = false;
            CancelReadyingBasicAttack();
        }
    }

    public void IssueAttackCommand(GameObject target)
    {
        currentCommand = CommandType.Attack;
        followTarget = target;
    }

    public void IssueAttackMoveCommand(Vector3 targetPosition)
    {
        Debug.Log("AttackMove is not yet implemented!");
    }

    void Update()
    {
        if (currentCommand != CommandType.Attack && currentCommand != CommandType.Spell && followTarget != null)
        {
            followTarget = null;
        }

        if (currentCommand != CommandType.Attack && isReadyingBasicAttack)
        {
            isReadyingBasicAttack = false;
            CancelReadyingBasicAttack();
        }

        if (currentCommand == CommandType.Attack && followTarget == null)
        {
            if (isReadyingBasicAttack)
            {
                isReadyingBasicAttack = false;
                CancelReadyingBasicAttack();
            }
            IssueStopCommand();
        }

        switch (currentCommand)
        {
            case CommandType.Attack:
                float distance = Vector3.Distance(transform.position, followTarget.transform.position);
                if (isReadyingBasicAttack)
                {
                    if (distance > basicAttackMaxRange) {
                        isReadyingBasicAttack = false;
                        CancelReadyingBasicAttack();
                    } else if (Time.time - basicAttackReadyStartTime >= basicAttackReadyTime)
                    {
                        isReadyingBasicAttack = false;
                        lastBasicAttackTime = Time.time;
                        BasicAttack();
                    }
                }
                else
                {
                    if (distance > basicAttackActivateRange)
                    {
                        m_nma.destination = followTarget.transform.position;
                    } else
                    {
                        m_nma.destination = transform.position;
                        if (Time.time - lastBasicAttackTime >= basicAttackCooldown)
                        {
                            basicAttackReadyStartTime = Time.time;
                            isReadyingBasicAttack = true;
                            StartReadyingBasicAttack();
                        }
                    }
                    

                }
                break;
            case CommandType.Move:
                break;
            case CommandType.Spell:
                if(followTarget != null)
                {
                    m_nma.destination = followTarget.transform.position;
                }
                break;
        }


    }
    public void StartReadyingBasicAttack()
    {

    }

    public void CancelReadyingBasicAttack()
    {

    }

    public void BasicAttack()
    {
        Debug.Log("Attak!");
    }
}
