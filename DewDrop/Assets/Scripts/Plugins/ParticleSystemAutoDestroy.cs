using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    ParticleSystem ps;

    private float destroyFlagTime;
    private void Awake()
    {
        destroyFlagTime = Time.time;
        ps = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if (Time.time - destroyFlagTime < 0.5f) return; // Grace Period
        if(ps == null || !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
