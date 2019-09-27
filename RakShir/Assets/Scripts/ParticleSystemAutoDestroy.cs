using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    ParticleSystem ps;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if(ps == null || !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
