using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Rare_GrantMagic : AbilityInstance
{
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        transform.parent = info.owner.transform;
        if (photonView.IsMine)
        {
            trg_Spell_Rare_MagicArrow trg = info.owner.control.skillSet[1] as trg_Spell_Rare_MagicArrow;
            if(trg != null)
            {
                trg.ResetCooldown();
                trg.shotArrows = 2;
            }
            

            
            Despawn();
        }

    }
}
