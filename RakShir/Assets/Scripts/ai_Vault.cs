using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Vault : AbilityInstance
{
    public float vaultDistance = 2f;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {

        float vaultSpeed = castInfo.owner.stat.finalMovementSpeed * 3;

        castInfo.owner.DashThroughWithSpeed(castInfo.owner.transform.position + castInfo.directionVector * vaultDistance, vaultSpeed);
        castInfo.owner.control.StartChanneling(vaultDistance / (vaultSpeed / 100), movable: true);
        castInfo.owner.control.basicAttackAbilityTrigger.ResetCooldown();


        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

}
