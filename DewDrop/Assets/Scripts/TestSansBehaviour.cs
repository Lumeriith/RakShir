using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TestSansBehaviour : MonoBehaviourPun
{
    private LivingThing livingThing;
    private void Start()
    {
        livingThing = GetComponent<LivingThing>();
        if (photonView.IsMine)
        {
            livingThing.OnDeath += Death;
        }
    }

    void Death(InfoDeath info)
    {
        livingThing.Revive();
        StatusEffect hot = new StatusEffect(livingThing, StatusEffectType.HealOverTime, 3, livingThing.maximumHealth);
        livingThing.statusEffect.ApplyStatusEffect(hot);

        StatusEffect invul = new StatusEffect(livingThing, StatusEffectType.Invulnerable, 3);
        livingThing.statusEffect.ApplyStatusEffect(invul);
    }
}
