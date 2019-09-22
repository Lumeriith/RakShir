using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingThing
{
    private void Awake()
    {
        
        bool isLocalPlayer = true;
        if (isLocalPlayer) UnitControlManager.instance.selectedUnit = this;
        currentHp = maxHp;
        spell = GetComponent<LivingThingSpell>();
    }



    protected override void Dead()
    {

    }
}
