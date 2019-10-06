using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StatusEffectParticleEffectAutoDestroy : MonoBehaviour
{
    public StatusEffect core;
    private ParticleSystem ps;


    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
        //Destroy Duplicate! TODO
    }

    private void FixedUpdate()
    {
        if (!core.isAlive)
        {
            ps.Stop();
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
