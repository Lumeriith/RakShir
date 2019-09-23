using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class trg_ConfettiBasicAttack : SpellTrigger
{
    public override void OnCast(SpellManager.CastInfo info)
    {
        SpellManager.CreateSpell("spl_ConfettiProjectile", info.owner.transform.position, info.owner.transform.rotation, info);
        StartCooldown();
    }

}
