using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Monster_BossNethergos_Meteor : AbilityTrigger
{
    public float channelDuration = 10f;
    public float preDelay = 2f;
    public int meteorCount = 20;
    public float meteorDuration = 12f;
    public float meteorRange = 20f;
    public float dropOnPlayerChance = 0.1f;
    public float onPlayerRange = 3f;
    public override void OnCast(CastInfo info)
    {
        info.owner.control.StartChanneling(new Channel(selfValidator, channelDuration, false, false, false, false, null, null));
        PlayerViewCamera.instance.visionMultiplier *= 1.35f;
        StartCooldown();
        StartCoroutine(CoroutineMeteor());
    }

    private IEnumerator CoroutineMeteor()
    {
        yield return new WaitForSeconds(preDelay);
        Vector3 delta;
        for(int i = 0; i < meteorCount; i++)
        {
            delta = Random.insideUnitCircle;
            delta.z = delta.y;
            delta.y = 0f;
            if (Random.value < dropOnPlayerChance)
            {
                CreateAbilityInstance("ai_Monster_BossNethergos_Meteor", info.target.transform.position + delta * onPlayerRange, Quaternion.identity);
            }
            else
            {
                CreateAbilityInstance("ai_Monster_BossNethergos_Meteor", info.owner.transform.position + delta * meteorRange, Quaternion.identity);
            }

            yield return new WaitForSeconds(meteorDuration / meteorCount);
        }
        PlayerViewCamera.instance.visionMultiplier /= 1.35f;

    }
}
