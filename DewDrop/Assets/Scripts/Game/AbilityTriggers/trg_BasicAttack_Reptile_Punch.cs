using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Reptile_Punch : AbilityTrigger
{
    [SerializeField]
    private float _channelTime = 0.23f;

    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, _channelTime, false, false, false, true, ChannelSuccess, ResetCooldown, true);
        owner.control.StartChanneling(channel);
        StartCooldown(true);
    }

    private void ChannelSuccess()
    {
        CreateAbilityInstance("ai_BasicAttack_Reptile_Punch", info.target.GetRandomOffset() + info.target.transform.position, Quaternion.identity, info);
    }
}
