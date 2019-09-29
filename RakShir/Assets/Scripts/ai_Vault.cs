using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Vault : AbilityInstance
{
    public float vaultDistance = 2f;
    public float vaultSpeed = 6f;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {

        castInfo.owner.DashThroughWithSpeed(castInfo.owner.transform.position + castInfo.directionVector * vaultDistance, vaultSpeed*100);
        castInfo.owner.control.StartChanneling(vaultDistance / vaultSpeed - 0.1f, movable: true);

        castInfo.owner.control.basicAttackAbilityTrigger.ResetCooldown();

        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
