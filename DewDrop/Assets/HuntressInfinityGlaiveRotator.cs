using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressInfinityGlaiveRotator : MonoBehaviour
{
    public float angularSpeed = 800;
    private float currentDegree = 0;
    void Update()
    {
        currentDegree += angularSpeed * Time.deltaTime;
        if (currentDegree > 360) currentDegree -= 360f;
        transform.rotation = Quaternion.Euler(new Vector3(0, currentDegree, 0));
    }
}
