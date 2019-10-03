using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
    protected float duration;

    protected abstract void BeginEffect();
    protected abstract void EndEffect();
}