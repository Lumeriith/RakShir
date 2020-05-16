using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Spell_Reptile_Leap : AbilityInstance
{
    public float dashDuration;
    public float thumpRadius = 0.75f;
    public float thumpPushDistance = 0.5f;
    public float thumpDamage = 50f;
    public float thumpPushDuration = 0.35f;
    public float afterThumpChannelTime = 0.25f;
    public TargetValidator thumpAffectedTargets;
    public SelfValidator afterThumpChannelValidator;

    public ParticleSystem jump;
    public ParticleSystem land;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        jump.Play();
        if (!isMine) return;
        info.owner.StartDisplacement(Displacement.ByVector(info.point - info.owner.transform.position, dashDuration, true, true, true, Ease.Linear, JumpFinished, JumpCanceled));
    }

    private void JumpFinished()
    {
        photonView.RPC(nameof(RpcLand), RpcTarget.All);
        SFXManager.CreateSFXInstance("si_Spell_Reptile_Leap Land", info.owner.transform.position);
        List<Entity> affectedTargets = info.owner.GetAllTargetsInRange(info.owner.transform.position, thumpRadius, thumpAffectedTargets);
        for(int i = 0; i < affectedTargets.Count; i++)
        {
            info.owner.DoMagicDamage(affectedTargets[i], thumpDamage, false, this);
            affectedTargets[i].StartDisplacement(Displacement.ByVector((affectedTargets[i].transform.position - info.owner.transform.position).normalized * thumpPushDistance, thumpPushDuration, false, false, false, Ease.EaseOutQuart));
            SFXManager.CreateSFXInstance("si_Spell_Reptile_Leap Hit", affectedTargets[i].transform.position);
        }
        info.owner.control.StartChanneling(new Channel(afterThumpChannelValidator, afterThumpChannelTime, false, false, false, false));
        Despawn();
    }

    private void JumpCanceled()
    {
        Despawn();
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }


    [PunRPC]
    private void RpcLand()
    {
        land.Play();
    }
}
