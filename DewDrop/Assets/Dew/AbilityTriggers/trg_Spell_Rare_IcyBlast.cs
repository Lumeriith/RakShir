using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_IcyBlast : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_IcyBlast", transform.position, Quaternion.identity, info);
        StartCooldown();
        SpendMana();
    }

    private void ChannelFinished()
    {
        
    }
}
