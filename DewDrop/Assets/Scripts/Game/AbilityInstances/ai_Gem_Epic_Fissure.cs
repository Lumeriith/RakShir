using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Epic_Fissure : AbilityInstance
{
    protected override void OnCreate(CastInfo info, object[] data)
    {
        StartCoroutine(CoroutineFissure());
    }

    private IEnumerator CoroutineFissure()
    {
        gem_Epic_Fissure fissure = (gem_Epic_Fissure)gem;
        yield return new WaitForSeconds(fissure.delay);
        transform.position = info.target.transform.position + info.target.GetCenterOffset();
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GetComponent<ParticleSystem>().Play();
        if (isMine)
        {
            info.owner.DoPureDamage(info.target, fissure.totalDamage, this);
            SFXManager.CreateSFXInstance("si_Gem_Epic_Fissure", info.target.transform.position);
            Despawn(info.target, AttachBehaviour.Full);
        }

        
    }
}
