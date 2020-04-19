using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_IceBolt : AbilityTrigger
{
    public Vector3 offset;
    public int iceboltCount = 3;
    public float preDelay = 0.1f;
    public float delayBetweenBolts = 0.25f;

    public override void OnCast(CastInfo info)
    {
        StartCoroutine("CoroutineIceBolts");
        SpendMana();
        StartCooldown();
        info.owner.control.StartChanneling(new Channel(selfValidator, iceboltCount * delayBetweenBolts + preDelay, true, true, true, false, null, ChannelCanceled));
    }

    private void ChannelCanceled()
    {
        StopCoroutine("CoroutineIceBolts");
    }

    IEnumerator CoroutineIceBolts()
    {
        yield return new WaitForSeconds(preDelay);
        for(int i = 0; i < iceboltCount; i++)
        {
            CreateAbilityInstance("ai_Spell_Rare_IceBolt", info.owner.transform.position + offset, info.directionQuaternion, info);
            yield return new WaitForSeconds(delayBetweenBolts);
        }
    }
    
}
