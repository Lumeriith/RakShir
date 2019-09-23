using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class trg_ConfettiBasicAttack : SpellTrigger
{
    private SpellManager.CastInfo castInfo;
    public override void OnCast(SpellManager.CastInfo info)
    {
        castInfo = info;
        info.owner.control.StartChanneling(0.125f, Pew, NoPew, true);
        
        StartCooldown();
    }

    private void Pew()
    {
        SpellManager.CreateSpell("spl_ConfettiProjectile", castInfo.owner.transform.position, castInfo.owner.transform.rotation, castInfo);
        castInfo.owner.control.ReserveSpellTrigger(this, Vector3.zero, Vector3.zero, castInfo.target);
    }

    private void NoPew()
    {
        ResetCooldown();
    }
}
