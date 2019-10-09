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

    private void Awake()
    {
        foreach(Transform t in transform)
        {
            t.Find("Placeholder").gameObject.SetActive(false);
        }
    }

    public void CreateParticleEffect(StatusEffect ce)
    {
        Transform target = transform.Find(System.Enum.GetName(typeof(StatusEffectType), ce.type)); 

        if(target != null)
        {
            GameObject effect = Instantiate(target.gameObject, ce.owner.transform.position, ce.owner.transform.rotation, ce.owner.transform);
            effect.transform.localScale = new Vector3(1f, (ce.owner.top.transform.position.y - ce.owner.bottom.transform.position.y)/2, 1f);
            effect.AddComponent<StatusEffectParticleEffectAutoDestroy>().core = ce;
        }
    }
    
}
