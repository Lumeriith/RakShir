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

    private bool isCurrentChannelBasicAttack = false;

    public AbilityTrigger basicAttackAbilityTrigger;

    public AbilityTrigger[] keybindings = new AbilityTrigger[4];


    [Header("Aggro Settings")]
    public bool aggroAutomatically;
    public float aggroRange;
    public float aggroChecksPerSecond;
    [Header("Deaggro Settings")]
    public bool deaggroAutomatically;
    public float deaggroRange;
    public float deaggroTime;
    

    private float deaggroCurrerntTime = 0;

    private float lastAggroCheckTime;

    [Header("Preconfigurations")]
    public LayerMask maskLivingThing;

    private const float modelTurnSpeed = 800;
    public Quaternion desiredRotation { get; private set; }

    public void StartChanneling(float channelTime, System.Action successCallback = null, System.Action canceledCallback = null, bool movable = false)
    {
        if(reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable || reservedAction == ActionType.SoftChannel)
        {
            CancelChanneling();
        }
        reservedAction = movable ? ActionType.ChannelMovable : ActionType.Channel;
        channelSuccessCallback = successCallback != null ? successCallback : () => { };
        channelCanceledCallback = canceledCallback != null ? canceledCallback : () => { };
        channelRemainingTime = channelTime;
        isCurrentChannelBasicAttack = false;
    }

    private void WalkCheck()
    {
        float difference = Vector3.Distance(navMeshAgentDestination, transform.position);
        if (wasWalking && (navMeshAgent.enabled == false || difference < float.Epsilon || navMeshAgent.speed == 0 || navMeshAgent.desiredVelocity.magnitude < 0.1f))
        {
            wasWalking = false;
            photonView.RPC("RpcStopWalking", RpcTarget.All);
        }
        else if (!wasWalking && navMeshAgent.enabled == true && difference > float.Epsilon && navMeshAgent.speed > 0 && navMeshAgent.desiredVelocity.magnitude > 0.1f)
        {
            wasWalking = true;
            photonView.RPC("RpcStartWalking", RpcTarget.All, navMeshAgentDestination);
        }
    }
    public void StartBasicAttackChanneling(float ratio, System.Action successCallback, System.Action canceledCallback, bool movable = false)
    {
        if (reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable || reservedAction == ActionType.SoftChannel)
        {
            CancelChanneling();
        }
        reservedAction = movable ? ActionType.ChannelMovable : ActionType.SoftChannel;
        channelSuccessCallback = successCallback;
        channelCanceledCallback = canceledCallback;
        channelRemainingTime = ratio * (1 / livingThing.stat.finalAttacksPerSecond) / ((100 + livingThing.statusEffect.totalHasteAmount) / 100f);
        isCurrentChannelBasicAttack = true;
        photonView.RPC("RpcChannelBasicAttack", RpcTarget.All, channelRemainingTime);
    }

    private void DoRotateTick()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, modelTurnSpeed * Time.deltaTime);
    }

    public void LookAt(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        desiredRotation = Quaternion.Euler(euler);
    }

    public void LookAt(Vector3 lookLocation)
    {
        if ((lookLocation - transform.position).magnitude <= float.Epsilon) return;
        LookAt(Quaternion.LookRotation(lookLocation - transform.position, Vector3.up));
    }

    public void ImmediatelySetRotation()
    {
        transform.rotation = desiredRotation;
    }

    public void CancelChanneling()
    {
        if (reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable || reservedAction == ActionType.SoftChannel)
        {
            if (isCurrentChannelBasicAttack) photonView.RPC("RpcChannelBasicAttackCanceled", RpcTarget.All, channelRemainingTime);
            reservedAction = ActionType.None;
            channelCanceledCallback.Invoke();
            isCurrentChannelBasicAttack = false;
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
        } else if (reservedAction == ActionType.ChannelMovable)
        {
            navMeshAgentDestination = location;
        }
        else
        {
            navMeshAgentDestination = location;
            reservedAction = ActionType.Move;
        }

        
        
    }

    public void StartAttackMoving(Vector3 location)
    {
        if (reservedAction == ActionType.SoftChannel)
        {
            if (isCurrentChannelBasicAttack)
            {
                
                DoAggroCheck();
                return;
            }
            else
            {
                CancelChanneling();
            }
            
        }
        else if (reservedAction == ActionType.Channel)
        {
            return;
        } else if (reservedAction == ActionType.ChannelMovable)
        {
            StartMoving(location);
            return;
        }

        if (!DoAggroCheck())
        {
            reservedAction = ActionType.AttackMove;
            lastAggroCheckTime = Time.time;
            navMeshAgentDestination = location;
        }

    }



    private bool DoAggroCheck()
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
            return true;
        }
        return false;
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
            navMeshAgent.speed = livingThing.stat.finalMovementSpeed / 100 * ((100 + livingThing.statusEffect.totalSpeedAmount) / 100f) * ((100 - livingThing.statusEffect.totalSlowAmount) / 100f);
        }
        else
        {
            navMeshAgent.speed = 0;
        }

        if (reservedAction == ActionType.Move && !SelfValidator.CanHaveMoveActionReserved.Evaluate(livingThing))
        {
            reservedAction = ActionType.None;
        }

        DoDeaggroCheck();

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
                if (navMeshAgent.enabled && navMeshAgent.path.corners.Length > 1)
                {
                    LookAt(navMeshAgent.path.corners[1]);
                }
                
                
                if (Vector3.Distance(navMeshAgentDestination, transform.position) < 0.2f)
                {
                    reservedAction = ActionType.None;
                }
                break;
            case ActionType.AttackMove:
                if (navMeshAgent.enabled && navMeshAgent.path.corners.Length > 1)
                {
                    LookAt(navMeshAgent.path.corners[1]);
                }
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
                navMeshAgentDestination = transform.position;
                HandleChannelTick();
                break;
            case ActionType.ChannelMovable:
                HandleChannelTick();
                break;
            case ActionType.SoftChannel:
                navMeshAgentDestination = transform.position;
                HandleChannelTick();
                break;
        }

        if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(navMeshAgentDestination);
        }
    }


    private bool wasWalking = false;
    private void LateUpdate()
    {
        WalkCheck();
    }

    private void HandleChannelTick()
    {
        channelRemainingTime = Mathf.MoveTowards(channelRemainingTime, 0, Time.deltaTime);
        if (channelRemainingTime <= 0)
        {
            reservedAction = ActionType.None;

            channelSuccessCallback.Invoke();

            isCurrentChannelBasicAttack = false;
        }
    }

    void DoDeaggroCheck()
    {
        if (!deaggroAutomatically) return;

        if (reservedAction == ActionType.AbilityTrigger && actionAbilityTrigger == basicAttackAbilityTrigger)
        {
            if (Vector3.Distance(transform.position, actionInfo.target.transform.position) > deaggroRange)
            {
                deaggroCurrerntTime += Time.deltaTime;
                if (deaggroCurrerntTime > deaggroTime)
                {
                    deaggroCurrerntTime = 0;
                    reservedAction = ActionType.None;
                    print("Deaggroed");
                    return;
                }
            }
            else
            {
                deaggroCurrerntTime = 0;
            }
        }
        else
        {
            deaggroCurrerntTime = 0;
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

        //if ((reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable) &&
        //    basicAttackAbilityTrigger == abilityTrigger && actionInfo.target == target) return;
        if (isCurrentChannelBasicAttack && actionAbilityTrigger == abilityTrigger && actionInfo.point == point && actionInfo.directionVector == directionVector && actionInfo.target == target) return;

        if (reservedAction == ActionType.Channel || reservedAction == ActionType.ChannelMovable) return;

        if (reservedAction == ActionType.SoftChannel) CancelChanneling();

        reservedAction = ActionType.AbilityTrigger;
        actionAbilityTrigger = abilityTrigger;

        actionInfo.point = point;
        actionInfo.directionVector = directionVector;
        actionInfo.target = target;
    }

    [PunRPC]
    private void RpcCast(int abilityIndex, int owner_id, Vector3 point, Vector3 directionVector, int target_id)
    {
        CastInfo info;
        info.owner = PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();
        info.point = point;
        info.directionVector = directionVector;
        info.target = target_id == -1 ? null : PhotonNetwork.GetPhotonView(owner_id).GetComponent<LivingThing>();

        InfoAbilityCast infoAbilityCast;
        infoAbilityCast.abilityIndex = abilityIndex;
        infoAbilityCast.castInfo = info;
        livingThing.OnAbilityCast.Invoke(infoAbilityCast);
    }

    private void AutoDoRpcCast()
    {
        for(int i = 0;i< keybindings.Length; i++)
        {
            if (keybindings[i] != null && keybindings[i] == actionAbilityTrigger)
            {
                if(actionInfo.target == null)
                {
                    photonView.RPC("RpcCast", RpcTarget.All, i, photonView.ViewID, actionInfo.point, actionInfo.directionVector, -1);
                }
                else
                {
                    photonView.RPC("RpcCast", RpcTarget.All, i, photonView.ViewID, actionInfo.point, actionInfo.directionVector, actionInfo.target.photonView.ViewID);
                }
                
                return;
            }
        }

        photonView.RPC("RpcCast", RpcTarget.All, -1, photonView.ViewID, actionInfo.point, actionInfo.directionVector, actionInfo.target.photonView.ViewID);


    }

    private void TryCastReservedAbilityInstance()
    {


        switch (actionAbilityTrigger.targetingType)
        {
            case AbilityTrigger.TargetingType.None:
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                AutoDoRpcCast();
                actionAbilityTrigger.Cast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.Direction:
                if (!actionAbilityTrigger.isCooledDown) break;


                LookAt(transform.position + actionInfo.directionVector);
                reservedAction = ActionType.None;
                AutoDoRpcCast();
                actionAbilityTrigger.Cast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.Target:
                
                Vector3 targetPosition = actionInfo.target.transform.position;
                
                if (Vector3.Distance(transform.position, targetPosition) <= actionAbilityTrigger.range)
                {
                    
                    navMeshAgentDestination = transform.position;
                    LookAt(targetPosition);
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    AutoDoRpcCast();
                    actionAbilityTrigger.Cast(actionInfo);
                    deaggroCurrerntTime = 0;

                }
                else
                {
                    navMeshAgentDestination = targetPosition;
                    if (navMeshAgent.enabled && navMeshAgent.path.corners.Length > 1)
                    {
                        LookAt(navMeshAgent.path.corners[1]);
                    }
                }
                
                break;
            case AbilityTrigger.TargetingType.PointNonStrict:
                navMeshAgentDestination = transform.position;
                LookAt(actionInfo.point);
                if (!actionAbilityTrigger.isCooledDown) break;
                reservedAction = ActionType.None;
                AutoDoRpcCast();
                actionAbilityTrigger.Cast(actionInfo);
                break;
            case AbilityTrigger.TargetingType.PointStrict:
                if (Vector3.Distance(transform.position, actionInfo.point) <= actionAbilityTrigger.range)
                {
                    navMeshAgentDestination = transform.position;
                    if (!actionAbilityTrigger.isCooledDown) break;
                    reservedAction = ActionType.None;
                    AutoDoRpcCast();
                    actionAbilityTrigger.Cast(actionInfo);
                    LookAt(actionInfo.point);
                }
                else
                {
                    navMeshAgentDestination = actionInfo.point;
                    if (navMeshAgent.enabled && navMeshAgent.path.corners.Length > 1)
                    {
                        LookAt(navMeshAgent.path.corners[1]);
                    }
                }
                
                break;
        }
    }
}
