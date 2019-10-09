using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerAnimation : MonoBehaviourPun
{
    LivingThing livingThing;
    Animator animator;
    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        animator = transform.Find("Model").GetComponent<Animator>();
    }

    private void Start()
    {
        livingThing.OnStartWalking += StartWalkingAnimation;
        livingThing.OnStopWalking += StopWalkingAnimation;
        livingThing.OnStartStunned += StartStunnedAnimation;
        livingThing.OnStopStunned += StopStunnedAnimation;
    }

    private void StartWalkingAnimation(InfoStartWalking info)
    {
        animator.SetBool("IsWalking", true);
    }

    private void StopWalkingAnimation(InfoStopWalking info)
    {
        animator.SetBool("IsWalking", false);
    }

    private void StartStunnedAnimation()
    {
        animator.SetBool("IsStunned", true);
    }

    private void StopStunnedAnimation()
    {
        animator.SetBool("IsStunned", false);
    }

}
