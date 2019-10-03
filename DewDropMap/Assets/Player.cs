using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingThing
{
    PlayerSpell spell;
    private void Awake()
    {
        spell = GetComponent<PlayerSpell>();
    }

    void Update()
    {
        
    }

    protected override void Dead()
    {

    }
}
