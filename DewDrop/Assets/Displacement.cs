using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Displacement
{
    private bool hasFinished = false;

    public bool isTargetDisplacement = false;

    public Vector3 vector1;
    public Vector3 vector2;
    public float duration;
    public LivingThing target;
    public LivingThing self;

    public bool isFriendly;
    public bool lookForward;
    public EasingFunction.Ease ease;
    private EasingFunction.Function easeFunction;

    private float elapsedTime = 0f;

    private UnityAction finishedCallback = null;
    private UnityAction canceledCallback = null;

    public void SetStartPosition(Vector3 position)
    {
        if (!isTargetDisplacement) return;
        vector1 = position;
        vector2 = position + vector2;
    }

    public void SetSelf(LivingThing self)
    {
        this.self = self;
    }

    public void Cancel()
    {
        if (hasFinished) return;
        if (canceledCallback != null) canceledCallback.Invoke();
        hasFinished = true;
    }

    // Warning: Callbacks are only called on local!
    Displacement(Vector3 displacementVector, float duration, bool isFriendly, bool lookForward, EasingFunction.Ease ease = EasingFunction.Ease.Linear, UnityAction finishedCallback=null, UnityAction canceledCallback=null)
    {
        isTargetDisplacement = false;
        this.vector2 = displacementVector;
        this.duration = duration;
        this.isFriendly = isFriendly;
        this.lookForward = lookForward;
        this.ease = ease;
        this.finishedCallback = finishedCallback;
        this.canceledCallback = canceledCallback;
    }

    // Warning: Callbacks are only called on local!
    Displacement(LivingThing to, float gap, float speed, bool isFriendly, bool lookForward, UnityAction finishedCallback, UnityAction canceledCallback)
    {
        isTargetDisplacement = true;
        target = to;
        vector1 = new Vector3(gap, speed);
        this.isFriendly = isFriendly;
        this.lookForward = lookForward;
        this.finishedCallback = finishedCallback;
        this.canceledCallback = canceledCallback;
    }

    public bool Tick()
    {
        if (hasFinished) return true;
        if (easeFunction == null) easeFunction = EasingFunction.GetEasingFunction(ease);

        
        if (isTargetDisplacement)
        {
            Vector3 targetPosition = target.transform.position + (self.transform.position - target.transform.position).normalized * vector1.x;
            self.transform.position =
                Vector3.MoveTowards(self.transform.position,
                targetPosition,
                vector1.y * Time.deltaTime);
            if (lookForward) self.RpcLookAt(target.transform.position, true);
            if(Vector3.Distance(self.transform.position, targetPosition) <= float.Epsilon)
            {
                if (finishedCallback != null) finishedCallback.Invoke();
                hasFinished = true;
                return true;
            }
        }
        else
        {
            float t = easeFunction(0f, 1f, elapsedTime / duration);
            self.transform.position = Vector3.Lerp(vector1, vector2, t);
            if (lookForward) self.RpcLookAt(vector2, true);
            if(elapsedTime >= duration)
            {
                if (finishedCallback != null) finishedCallback.Invoke();
                hasFinished = true;
                return true;
            }
        }

        elapsedTime += Time.deltaTime;
        return hasFinished;
    }

}
