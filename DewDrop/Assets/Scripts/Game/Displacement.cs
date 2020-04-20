using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class Displacement
{
    public enum DisplacementType : byte
    {
        TowardsTarget, ByVector
    }

    // Common Displacement Properties
    public DisplacementType type { get; private set; }
    public bool isFriendly { get; private set; }
    public bool lookForward { get; private set; }

    private System.Action _finishedCallback = () => { };
    private System.Action _canceledCallback = () => { };

    // TowardsTarget Displacement Properties
    public LivingThing to { get; private set; }
    public float gap { get; private set; }
    public float speed { get; private set; }

    // ByVector Displacement Properties
    public Vector3 vector { get; private set; }
    public float duration { get; private set; }
    public bool ignoreCollision { get; private set; }
    public Ease ease { get; private set; }

    // Active Displacement Properties
    public LivingThing self { get; private set; }
    public bool hasFinished { get; private set; }




    private EaseFunction easeFunctionDerivative
    {
        get
        {
            if (_easeFunction == null) _easeFunction = EasingFunction.GetEasingFunctionDerivative(ease);
            return _easeFunction;
        }
    }
    private EaseFunction _easeFunction;

    private float _elapsedTime = 0f;

    public static Displacement TowardsTarget(LivingThing to, float gap, float speed, bool isFriendly, bool lookForward, System.Action finishedCallback = null, System.Action canceledCallback = null)
    {
        Displacement newDisplacement = new Displacement();
        newDisplacement.type = DisplacementType.TowardsTarget;
        newDisplacement.to = to;
        newDisplacement.gap = gap;
        newDisplacement.speed = speed;
        newDisplacement.isFriendly = isFriendly;
        newDisplacement.lookForward = lookForward;
        newDisplacement._finishedCallback = finishedCallback;
        newDisplacement._canceledCallback = canceledCallback;
        return newDisplacement;
    }

    public static Displacement ByVector(Vector3 vector, float duration, bool isFriendly, bool lookForward, bool ignoreCollision, Ease ease = Ease.Linear, System.Action finishedCallback = null, System.Action canceledCallback = null)
    {
        Displacement newDisplacement = new Displacement();
        newDisplacement.type = DisplacementType.ByVector;
        newDisplacement.vector = vector;
        newDisplacement.duration = duration;
        newDisplacement.isFriendly = isFriendly;
        newDisplacement.lookForward = lookForward;
        newDisplacement.ignoreCollision = ignoreCollision;
        newDisplacement.ease = ease;
        newDisplacement._finishedCallback = finishedCallback;
        newDisplacement._canceledCallback = canceledCallback;
        return newDisplacement;
    }

    public bool Tick()
    {
        if (hasFinished) return true;
        
        if (type == DisplacementType.TowardsTarget)
        {
            Vector3 destination = to.transform.position + (self.transform.position - to.transform.position).normalized * gap;
            self.transform.position = Vector3.MoveTowards(self.transform.position, destination, speed * Time.deltaTime);
            if (lookForward) self.RpcLookAt(to.transform.position, true);
            if(Vector3.Distance(self.transform.position, destination) <= float.Epsilon)
            {
                _finishedCallback?.Invoke();
                hasFinished = true;
                return true;
            }
        }
        else if (type == DisplacementType.ByVector)
        {
            float t = easeFunctionDerivative(0f, 1f, _elapsedTime / duration);
            Vector3 delta = vector / duration * t * Time.deltaTime;

            if (ignoreCollision) self.transform.position += delta;
            else self.control.agent.Move(delta);

            if (lookForward) self.RpcLookAt(self.transform.position + vector, true);
            if (_elapsedTime >= duration)
            {
                if (_finishedCallback != null) _finishedCallback.Invoke();
                hasFinished = true;
                return true;
            }
        }

        _elapsedTime += Time.deltaTime;
        return hasFinished;
    }

    public void Cancel()
    {
        if (hasFinished) return;
        _canceledCallback?.Invoke();
        if (self.ongoingDisplacement == this) self.ongoingDisplacement = null;
        hasFinished = true;
    }

    public void SetSelf(LivingThing self)
    {
        this.self = self;
    }

}
