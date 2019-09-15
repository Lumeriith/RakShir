using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spl_ConfettiBlink : Spell
{
    ParticleSystem ps;
    float startTime = 0;
    bool blinked = false;
    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        startTime = Time.time;
    }

    void Update()
    {
        if (!ps.IsAlive())
        {
            Destroy(gameObject);
        }
        if(!blinked && startTime > 0.5f)
        {
            owner.transform.position = point + Vector3.up * .5f;
            blinked = false;
            ps.Play();
        }
    }
}
