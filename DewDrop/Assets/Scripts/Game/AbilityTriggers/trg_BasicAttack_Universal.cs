using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_BasicAttack_Universal : AbilityTrigger
{
    [Header("Universal Basicattack Settings")]
    public float channelDuration = 0.3f;
    public string abilityInstance = "ai_BasicAttack_Rare_";
    public List<GameObject> channelFinishedSoundEffects; 
    public override void OnCast(CastInfo info)
    {
        Channel channel = new Channel(selfValidator, channelDuration, false, false, false, true, ChannelSuccess, ResetCooldown, true);
        info.owner.control.StartChanneling(channel);
        StartCooldown(true);
    }


    private void ChannelSuccess()
    {
        CreateAbilityInstance(abilityInstance, info.owner.transform.position + info.owner.GetCenterOffset(), Quaternion.identity, info);
        if(channelFinishedSoundEffects != null && channelFinishedSoundEffects.Count != 0)
        {
            SFXManager.CreateSFXInstance(channelFinishedSoundEffects[Random.Range(0, channelFinishedSoundEffects.Count)].name, owner.transform.position);
        }
    }
}
