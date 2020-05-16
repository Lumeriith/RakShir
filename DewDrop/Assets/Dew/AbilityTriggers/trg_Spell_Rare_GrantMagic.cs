using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_GrantMagic : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        CreateAbilityInstance("ai_Spell_Rare_GrantMagic", info.owner.transform.position + Vector3.up, Quaternion.identity);
        StartCooldown();
        SpendMana();
    }
}
