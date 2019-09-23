using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
public class Player : LivingThing
{

    private void Awake()
    {
        control = GetComponent<LivingThingControl>();
        if (photonView.IsMine)
        {
            gameObject.layer = 10; // 12: Enemy -> 10: Player
            GetComponent<NavMeshAgent>().avoidancePriority++; // Local Player has the lowest priority
        }
        else
        {
            GetComponent<LivingThingControl>().enabled = false;
        }
        currentHp = maxHp;
        
    }

    protected override void Dead()
    {

    }


}
