using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalDamage : MonoBehaviour
{
    private List<Entity> affectedLivingThings = new List<Entity>();
    private List<float> lastHitTime = new List<float>();

    private bool didUpdate = false;

    public float amount = 10f;
    public float tickTime = 0.5f;
    private void OnTriggerStay(Collider other)
    {
        Entity thing = other.GetComponent<Entity>();
        if (thing == null) return;
        if (!thing.photonView.IsMine) return;
        int index = affectedLivingThings.IndexOf(thing);
        if (index != -1)
        {
            if (Time.time - lastHitTime[index] > tickTime)
            {
                affectedLivingThings[index].DoPureDamage(affectedLivingThings[index], amount, null);
                lastHitTime[index] = Time.time;
            }
        }
        else
        {
            affectedLivingThings.Add(thing);
            lastHitTime.Add(Time.time);
            thing.DoPureDamage(thing, amount, null);
        }
    }

    private void FixedUpdate()
    {
        for (int i = lastHitTime.Count - 1; i >= 0; i--)
        {
            if (Time.time - lastHitTime[i] > tickTime)
            {
                affectedLivingThings.RemoveAt(i);
                lastHitTime.RemoveAt(i);
            }
        }
    }
}
