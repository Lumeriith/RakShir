using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMarkerAutoDestroyer : MonoBehaviour
{
    private ParticleSystem m_particleSystem;
    private void Awake()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if (!m_particleSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
