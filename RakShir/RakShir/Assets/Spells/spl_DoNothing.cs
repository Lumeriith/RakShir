using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class spl_DoNothing : Spell
{

    void Start()
    {
        Destroy(gameObject);
    }

}
