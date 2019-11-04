using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_Blink : AbilityTrigger
{
    public float healthThreshold = 0.6f;
    public float cooldownReduction = 8f;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.2f, false, false, false, false, ChannelFinished, null));
        
        StartCooldown();
        if (info.owner.currentHealth >= info.owner.maximumHealth * healthThreshold) ApplyCooldownReduction(cooldownReduction);
        SpendMana();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Rare_Blink", transform.position, Quaternion.identity);
    }
}
