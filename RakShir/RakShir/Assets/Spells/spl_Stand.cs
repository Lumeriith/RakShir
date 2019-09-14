using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class spl_Stand : Spell
{
    public override bool CanBeCast()
    {
        return true;
    }
    void Start()
    {
        Destroy(gameObject);
    }

}
