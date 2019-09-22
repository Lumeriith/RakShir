using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
public class Player : LivingThing
{
    private void Awake()
    {
        if (photonView.IsMine)
        {
            gameObject.layer = 10; // 12: Enemy -> 10: Player
            GetComponent<NavMeshAgent>().avoidancePriority++; // Local Player has the lowest priority
        }
        else
        {
            GetComponent<PlayerSpell>().enabled = false;
        }
        currentHp = maxHp;
        spell = GetComponent<LivingThingSpell>();
    }

   

    protected override void Dead()
    {

    }
}
