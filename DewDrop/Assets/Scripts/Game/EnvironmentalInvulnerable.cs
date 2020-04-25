using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalInvulnerable : MonoBehaviour
{

    private List<Entity> affectedLivingThings = new List<Entity>();
    private List<StatusEffect> statusEffects = new List<StatusEffect>();
    private List<float> lastUpdateTime = new List<float>();

    private bool didUpdate = false;

    public float duration = 0.4f;
    public float tickTime = 0.2f;
    private void OnTriggerStay(Collider other)
    {
        Entity thing = other.GetComponent<Entity>();
        if (thing == null) return;
        if (!thing.photonView.IsMine) return;
        int index = affectedLivingThings.IndexOf(thing);
        if (index != -1)
        {
            if(Time.time - lastUpdateTime[index] > tickTime)
            {
                statusEffects[index].SetDuration(duration);
                lastUpdateTime[index] = Time.time;
            }
        }
        else
        {
            affectedLivingThings.Add(thing);
            statusEffects.Add(StatusEffect.Invulnerable(duration));
            //thing.ApplyStatusEffect(statusEffects[statusEffects.Count - 1]);
            // TODO durationless statuseffect
            lastUpdateTime.Add(Time.time);
        }
    }

    private void FixedUpdate()
    {
        for(int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (!statusEffects[i].isAlive)
            {
                affectedLivingThings.RemoveAt(i);
                statusEffects.RemoveAt(i);
                lastUpdateTime.RemoveAt(i);
            }
        }
    }





}
