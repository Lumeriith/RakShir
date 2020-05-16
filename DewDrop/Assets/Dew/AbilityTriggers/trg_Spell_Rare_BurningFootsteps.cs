using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trg_Spell_Rare_BurningFootsteps : AbilityTrigger
{
    public float duration = 4f;
    public float speedAmount = 15f;
    public float minimumDistance = 0.75f;

    private bool isCoroutineRunning = false;

    public override void OnCast(CastInfo info)
    {
        StartCoroutine(CoroutineMakeFire(info));
        StartCooldown();
        SpendMana();
        info.owner.ApplyStatusEffect(StatusEffect.Speed(duration, speedAmount), null);
        // TODO better implementation
        isCoroutineRunning = true;
    }

    private IEnumerator CoroutineMakeFire(CastInfo info)
    {
        List<Vector3> positions = new List<Vector3>();
        float startTime = Time.time;
        bool dontSpawn = false;
        while (Time.time - startTime < duration)
        {
            yield return new WaitForSeconds(0.2f);
            dontSpawn = false;
            for(int i = 0; i < positions.Count; i++)
            {
                if(Vector3.Distance(info.owner.transform.position, positions[i]) < minimumDistance)
                {
                    dontSpawn = true;
                    break;
                }
            }
            if (!dontSpawn)
            {
                CreateAbilityInstance("ai_Spell_Rare_BurningFootsteps", info.owner.transform.position, Quaternion.identity, info);
                positions.Add(info.owner.transform.position);
            }
            
        }
        isCoroutineRunning = false;
    }
    public override bool IsReady()
    {
        return !isCoroutineRunning;
    }

    public override void OnUnequip()
    {
        StopAllCoroutines();
        isCoroutineRunning = false;
    }
}
