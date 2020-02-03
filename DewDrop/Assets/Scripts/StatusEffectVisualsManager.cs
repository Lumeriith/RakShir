using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectVisualsManager : MonoBehaviour
{
    public List<StatusEffectVisual> visuals;

    private static StatusEffectVisualsManager _instance;
    
    public static void CreateVisual(LivingThing owner, StatusEffectType type)
    {
        if (_instance == null) _instance = FindObjectOfType<StatusEffectVisualsManager>();
        for (int i = 0; i < _instance.visuals.Count; i++)
        {
            if (_instance.visuals[i].type != type) continue;
            StatusEffectVisual newVisual = Instantiate(_instance.visuals[i].gameObject).GetComponent<StatusEffectVisual>();
            newVisual.Attach(owner);
        }
    }
}
