using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Elemental_DoubleKick : AbilityTrigger
{
    CastInfo info;
    public override void OnCast(CastInfo info)
    {
        this.info = info;
        
        ai_Spell_Elemental_DoubleKick spell = (ai_Spell_Elemental_DoubleKick)Resources.Load("ai_Spell_Elemental_DoubleKick");

        List<LivingThing> targets = info.owner.GetAllTargetsInRange(info.owner.transform.position, spell.range, spell.targetValidator);

        if (targets.Count == 0) return;


        object[] data = { targets[0].photonView.ViewID };
        StartCooldown();
        SpendMana();
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_DoubleKick", owner.transform.position, Quaternion.identity, info, data);
    }


}
