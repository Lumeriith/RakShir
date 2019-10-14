using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Elemental_DoubleKick : AbilityTrigger
{
    public float kickRange;
    public TargetValidator kickTargetValidator;
    public override void OnCast(CastInfo info)
    {
        

        List<LivingThing> targets = info.owner.GetAllTargetsInRange(info.owner.transform.position, kickRange, kickTargetValidator);

        if (targets.Count == 0) return;


        object[] data = { targets[0].photonView.ViewID };
        StartCooldown();
        SpendMana();
        AbilityInstanceManager.CreateAbilityInstance("ai_Spell_Elemental_DoubleKick", owner.transform.position, Quaternion.identity, info, data);
    }


}
