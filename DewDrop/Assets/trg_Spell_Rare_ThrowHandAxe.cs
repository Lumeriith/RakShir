using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_ThrowHandAxe : AbilityTrigger
{
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, 0.75f, false, false, false, false, ChannelFinished, ChannelCanceled));
        StartCooldown();
        SpendMana();
    }

    private void ChannelFinished()
    {
        CreateAbilityInstance("ai_Spell_Rare_ThrowHandAxe", info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity);
    }

    private void ChannelCanceled()
    {
        StartCooldown(10f);
        RefundMana();
    }
}
