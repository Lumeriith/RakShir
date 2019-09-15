using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spl_Confetti : Spell
{
    ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        transform.LookAt(target.transform);
    }

    void Update()
    {
        if (!ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
