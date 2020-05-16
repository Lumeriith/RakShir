using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Huntress_Ambush : AbilityTrigger
{
    private Entity huntressMarkTarget;

    private Entity dashTarget;

    public float backoffLifetime = 3f;

    private float dashStartTime = -1;

    public TargetValidator huntressMarkTargetValidator;
    public override void OnCast(CastInfo info)
    {
        if(dashTarget == null)
        {
            if (huntressMarkTarget == null) return;
            info.target = huntressMarkTarget;
            dashTarget = huntressMarkTarget;
            dashStartTime = Time.time;
            CreateAbilityInstance("ai_Spell_Huntress_Ambush", transform.position, Quaternion.identity, info);
            SpendMana();
        }
        else
        {
            info.target = dashTarget;
            CreateAbilityInstance("ai_Spell_Huntress_Ambush_Backoff", transform.position, Quaternion.identity, info);
            dashTarget = null;
            StartCooldown();
        }
    }

    public override void OnEquip()
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDoBasicAttackHit += HuntressMark;
        }
    }

    public override void OnUnequip()
    {
        if (owner.photonView.IsMine)
        {
            owner.OnDoBasicAttackHit -= HuntressMark;
            huntressMarkTarget = null;
        }
    }

    private void Update()
    {
        if(huntressMarkTarget != null)
        {
            if(!huntressMarkTargetValidator.Evaluate(owner, huntressMarkTarget))
            {
                huntressMarkTarget = null;
            }
        }

        if(dashTarget != null)
        {
            if (dashTarget.IsDead() || Time.time - dashStartTime > backoffLifetime)
            {
                dashTarget = null;
                StartCooldown();
            }
            else
            {
                SetSpecialFillAmount((backoffLifetime - (Time.time - dashStartTime)) / backoffLifetime);
            }
        }
        else
        {
            SetSpecialFillAmount(0);
        }


    }

    public override bool IsReady()
    {
        return huntressMarkTarget != null || dashTarget != null;
    }

    private void HuntressMark(InfoBasicAttackHit info)
    {
        if (!huntressMarkTargetValidator.Evaluate(info.from, info.to)) return;

        huntressMarkTarget = info.to;
    }


}
