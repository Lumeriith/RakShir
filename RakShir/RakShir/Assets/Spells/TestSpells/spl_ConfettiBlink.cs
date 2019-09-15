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
        if (blinked && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
        if (!blinked && Time.time - startTime > .25f)
        {
            transform.position = point;
            owner.transform.position = point + Vector3.up;
            blinked = true;
            ps.Play();
        } 
    }
}
