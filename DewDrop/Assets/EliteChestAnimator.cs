using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteChestAnimator : MonoBehaviour
{
    public AnimationCurve curve;
    public Transform targetTransform;
    public Transform closedTransform;
    public Transform openTransform;
    public float animationDuration = 1.5f;

    private void Awake()
    {
        LivingThing thing = GetComponent<LivingThing>();
        thing.OnDeath += Death;
        thing.LookAt(Camera.main.transform.position, true);
    }


    private void Death(InfoDeath info)
    {
        StartCoroutine(CoroutineAnimate());
    }

    private IEnumerator CoroutineAnimate()
    {
        float start = Time.time;
        while (Time.time - start < animationDuration)
        {
            targetTransform.position = Vector3.LerpUnclamped(closedTransform.position, openTransform.position, curve.Evaluate((Time.time - start) / animationDuration));
            targetTransform.rotation = Quaternion.LerpUnclamped(closedTransform.rotation, openTransform.rotation, curve.Evaluate((Time.time - start) / animationDuration));
            yield return null;
        }
    }
}
