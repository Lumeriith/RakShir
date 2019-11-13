using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalDamage : MonoBehaviour
{
    private List<LivingThing> affectedLivingThings = new List<LivingThing>();
    private List<float> lastHitTime = new List<float>();

    private bool didUpdate = false;

    public float amount = 10f;
    public float tickTime = 0.5f;
    private void OnTriggerStay(Collider other)
    {
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null) return;
        if (!thing.photonView.IsMine) return;
        int index = affectedLivingThings.IndexOf(thing);
        if (index != -1)
        {
            if (Time.time - lastHitTime[index] > tickTime)
            {
                affectedLivingThings[index].DoPureDamage(amount, affectedLivingThings[index]);
                lastHitTime[index] = Time.time;
            }
        }
        else
        {
            affectedLivingThings.Add(thing);
            lastHitTime.Add(Time.time);
            thing.DoPureDamage(amount, thing);
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
