using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spl_ConfettiProjectile : Spell
{
    protected override void OnCreate(SpellManager.CastInfo castInfo, object[] data)
    {
        Debug.Log(castInfo.owner);
        Debug.Log(data);
    }
}
