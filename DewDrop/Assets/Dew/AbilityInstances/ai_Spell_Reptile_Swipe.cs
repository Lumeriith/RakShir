using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Reptile_Swipe : AbilityInstance
{
    public float dashDistance = 2f;
    public float dashDuration = 0.65f;
    public float dashDamage = 60f;
    public TargetValidator affectedTargets;
    public int maxAttacks = 2;
    public float expireAfter = 5f;
    public float attackSpeedBonus = 100f;
    public GameObject hitEffect;

    private int _remainingAttacks;
    private StatusEffect _effect;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        entity.StartDisplacement(Displacement.ByVector(info.directionVector * dashDistance, dashDuration, true, true, false, Ease.EaseOutQuart, DashFinished, DashCanceled));
        _effect = StatusEffect.Haste(expireAfter, attackSpeedBonus);
        entity.ApplyStatusEffect(_effect, this);
        _remainingAttacks = maxAttacks;
        entity.OnDoBasicAttackHit += DidBasicAttackHit;
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
    }

    private void DidBasicAttackHit(InfoBasicAttackHit info)
    {
        if (!_effect.isAlive)
        {
            entity.OnDoBasicAttackHit -= DidBasicAttackHit;
            return;
        }
        _remainingAttacks--;
        if(_remainingAttacks == 0)
        {
            _effect.Remove();
            entity.OnDoBasicAttackHit -= DidBasicAttackHit;
        }
    }

    private void DashFinished()
    {
        Despawn();
    }

    private void DashCanceled()
    {
        Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive || !isMine || !other.IsAppropriateEntity(info.owner, affectedTargets, out Entity entity)) return;
        info.owner.DoMagicDamage(entity, dashDamage, false, this);
        SFXManager.CreateSFXInstance("si_Spell_Reptile_Swipe Hit", entity.transform.position);
        photonView.RPC(nameof(RpcCreateHitEffect), RpcTarget.All, entity.photonView.ViewID);
    }

    [PunRPC]
    private void RpcCreateHitEffect(int entityViewID)
    {
        Entity entity = Entity.GetFromViewID(entityViewID);
        PlayNew(hitEffect, entity.transform.position);
    }
}
