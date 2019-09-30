using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Photon.Pun;
using UnityEngine.Events;


public enum CommandType { Move, Attack, AttackMove, Chase, AutoChase, Ability }
public enum CommandLockType { DisallowAll, OnlyAllowMove, AllowEverything }





[System.Serializable]
public class Channel
{
    public SelfValidator channelValidator;
    public bool shouldCancelForOtherCommand;
    public bool canMove;
    public float duration;

    public UnityAction finishedCallback = null;
    public UnityAction canceledCallback = null;

    public CommandLockType commandLock;
    

    private bool hasEnded = false;

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

public class Command
{
    public CommandType type;
    public object[] parameters;

    private LivingThing self;

    public Command(LivingThing self, CommandType type, params object[] parameters)
    {
        this.self = self;
        this.type = type;
        this.parameters = parameters;
    }
    
    public float attackMoveTargetChecksForSecond = 4f;
    public float attackMoveTargetCheckRadius = 4f;

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
        }

        return false;
    }

    private bool ProcessMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
        self.control.agent.destination = destination;
        if (self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessAttack(LivingThing target)
    {
        if (self.control.skillSet[0] == null) return true;
        if (Vector3.Distance(self.transform.position, target.transform.position) > self.control.skillSet[0].range) return true;
        if (!self.control.skillSet[0].selfValidator.Evaluate(self)) return true;
        if (!self.control.skillSet[0].isCooledDown) return false;

        CastInfo info = new CastInfo { owner = self, directionVector = Vector3.zero, point = Vector3.zero, target = target };
        self.control.skillSet[0].Cast(info);
        return true;
    }

    private bool ProcessAttackMove(Vector3 destination)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;

        if (!self.control.skillSet[0].selfValidator.Evaluate(self))
        {
            self.control.agent.destination = destination;
            return false;
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(self.transform.position, 4f, LayerMask.GetMask("LivingThing"));
            float distance 


        }

        
        
        if (self.control.agent.desiredVelocity.magnitude < float.Epsilon) return true;
        return false;
    }

    private bool ProcessChase(LivingThing target)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
    }

    private bool ProcessAutoChase(LivingThing target)
    {
        if (SelfValidator.CancelsMoveCommand.Evaluate(self)) return true;
    }

    private bool ProcessAbility(AbilityTrigger trigger, CastInfo info)
    {

    }

    
}



public class LivingThingControl : MonoBehaviourPun
{
    private LivingThing livingThing;
    public NavMeshAgent agent { get; private set; }

    private List<Command> reservedCommands = new List<Command>();
    private Command currentCommand { get { return reservedCommands.Count == 0 ? null : reservedCommands[0]; } }
    private List<Channel> ongoingChannels = new List<Channel>();

    public AbilityTrigger[] skillSet = new AbilityTrigger[6];

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

    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        
        if(currentCommand != null)
        {
            bool isCommandFinished = currentCommand.Process();
            if (isCommandFinished) reservedCommands.RemoveAt(0);
        }


    }



}
