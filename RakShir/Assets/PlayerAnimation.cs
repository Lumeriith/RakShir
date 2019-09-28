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

        livingThing.OnChannelBasicAttack += StartBasicAttackAnimation;
        livingThing.OnStartWalking += StartWalkingAnimation;
        livingThing.OnStopWalking += StopWalkingAnimation;
        livingThing.OnAbilityCast += CastSpellAnimation;
    }

    private void StartBasicAttackAnimation(InfoChannel info)
    {
        animator.SetTrigger("Attack");
    }

    private void StartWalkingAnimation(InfoStartWalking info)
    {
        animator.SetBool("IsWalking", true);
    }

    private void StopWalkingAnimation(InfoStopWalking info)
    {
        animator.SetBool("IsWalking", false);
    }

    private void CastSpellAnimation(InfoAbilityCast info)
    {
        switch (info.abilityIndex)
        {
            case 0:
                CastWeaponSpellAnimation();
                break;
            case 1:
                CastArmorSpellAnimation();
                break;
            case 2:
                CastBootsSpellAnimation();
                break;
            case 3:
                CastWeaponUltimateSpellAnimation();
                break;
            case 4:
                CastRingSpellAnimation();
                break;
        }
    }

    private void CastWeaponSpellAnimation() // Q
    {
        animator.SetTrigger("CastWeaponSpell");
    }

    private void CastArmorSpellAnimation() // W
    {
        animator.SetTrigger("CastArmorSpell");
    }

    private void CastBootsSpellAnimation() // E
    {
        animator.SetTrigger("CastBootsSpell");
    }

    private void CastWeaponUltimateSpellAnimation() // R
    {
        animator.SetTrigger("CastWeaponUltimateSpell");
    }

    private void CastRingSpellAnimation() // Space
    {
        animator.SetTrigger("CastRingSpell");
    }



}
