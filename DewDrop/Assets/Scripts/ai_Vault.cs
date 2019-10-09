using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Vault : AbilityInstance
{
    public SelfValidator channelValidator;
    public float vaultDistance = 2f;
    public float vaultSpeed = 6f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {

        castInfo.owner.DashThroughWithSpeed(castInfo.owner.transform.position + castInfo.directionVector * vaultDistance, vaultSpeed*100);
        Channel channel = new Channel(channelValidator, vaultDistance / vaultSpeed - 0.1f, false, false, false, false, null, castInfo.owner.CancelDash);
        castInfo.owner.control.StartChanneling(channel);

        castInfo.owner.control.skillSet[0].ResetCooldown();

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
