using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gem_Rare_Immolation : Gem
{
    public TargetValidator burnableTargets;

    public float radius = 3f;
    public float tickTime = 0.5f;
    public float immolationDuration = 4.1f;
    public float[] damagePerTick = { 5, 10, 15, 20 };
    public float timeLeft = 0f;

    public override void OnGemActivate(Entity owner, AbilityTrigger trigger)
    {
        timeLeft = 0f;
    }

    public override void OnGemDeactivate(Entity owner, AbilityTrigger trigger)
    {
        timeLeft = 0f;
    }

    public override void AliveUpdate(bool isMine)
    {
        if(isMine) timeLeft = Mathf.MoveTowards(timeLeft, 0f, Time.deltaTime);
    }

    public override void OnTriggerCast(bool isMine)
    {
        if (!isMine) return;
        timeLeft = immolationDuration;
        if (!IsAnyInstanceActive()) CreateAbilityInstance("ai_Gem_Rare_Immolation", owner.transform.position, Quaternion.identity);
    }
}
