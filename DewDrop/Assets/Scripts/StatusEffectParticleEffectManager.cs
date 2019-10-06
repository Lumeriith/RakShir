using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectParticleEffectManager : MonoBehaviour
{


    private static StatusEffectParticleEffectManager _instance;
    public static StatusEffectParticleEffectManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StatusEffectParticleEffectManager>();
            }
            return _instance;
        }
    }

    public void CreateParticleEffect(StatusEffect ce)
    {
        Transform target = transform.Find(System.Enum.GetName(typeof(StatusEffectType), ce.type)); 

        if(target != null)
        {
            Instantiate(target.gameObject, ce.owner.top.position, ce.owner.transform.rotation, ce.owner.transform).AddComponent<StatusEffectParticleEffectAutoDestroy>().core = ce;
        }
    }
    
}
