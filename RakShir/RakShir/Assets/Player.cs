using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    PlayerSpell spell;
    private void Awake()
    {
        spell = GetComponent<PlayerSpell>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
