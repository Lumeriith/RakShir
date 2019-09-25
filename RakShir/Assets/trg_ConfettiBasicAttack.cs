using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class trg_ConfettiBasicAttack : AbilityTrigger
{
    private AbilityInstanceManager.CastInfo castInfo;
    public override void OnCast(AbilityInstanceManager.CastInfo info)
    {
        castInfo = info;
        info.owner.control.StartBasicAttackChanneling(1/3, Pew, NoPew);
        
        StartBasicAttackCooldown();
    }

    private void Pew()
    {
        AbilityInstanceManager.CreateAbilityInstance("spl_ConfettiProjectile", castInfo.owner.transform.position, castInfo.owner.transform.rotation, castInfo);
        castInfo.owner.control.ReserveAbilityTrigger(this, Vector3.zero, Vector3.zero, castInfo.target);
    }

    private void NoPew()
    {
        ResetCooldown();
    }
}
