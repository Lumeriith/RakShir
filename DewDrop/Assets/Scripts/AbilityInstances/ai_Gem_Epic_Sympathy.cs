using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_Gem_Epic_Sympathy : AbilityInstance
{
    private ParticleSystem distanceEmitter;

    private gem_Epic_Sympathy sympathy;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        distanceEmitter = transform.Find<ParticleSystem>("Distance Emitter");
        sympathy = (gem_Epic_Sympathy)source.gem;
        transform.position = info.target.transform.position + info.target.GetCenterOffset();
        transform.rotation = Quaternion.LookRotation(info.owner.transform.position - info.target.transform.position, Vector3.up);
        StartCoroutine(CoroutineSympathy());
        if (!isMine) return;
        SFXManager.CreateSFXInstance("si_Gem_Epic_Sympathy " + Random.Range(0, 3), Vector3.Lerp(info.target.transform.position + info.target.GetCenterOffset(), info.owner.transform.position + info.owner.GetCenterOffset(), .5f));

        List<LivingThing> targets = info.owner.GetAllTargetsInLine(
            info.target.transform.position + info.target.GetCenterOffset(),
            info.owner.transform.position + info.owner.GetCenterOffset() - info.target.transform.position - info.target.GetCenterOffset(),
            sympathy.splashWidth,
            Vector3.Distance(info.owner.transform.position + info.owner.GetCenterOffset(), info.target.transform.position + info.target.GetCenterOffset()),
            sympathy.affectedTargets);
        for(int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == info.target) continue;
            info.owner.DoMagicDamage((float)data[0] * sympathy.splashPercentage[sympathy.level] / 100f, targets[i], true, source);
            SFXManager.CreateSFXInstance("si_Gem_Epic_Sympathy Hit", targets[i].transform.position + targets[i].GetCenterOffset());
        }
    }

    private IEnumerator CoroutineSympathy()
    {
        distanceEmitter.Play();
        yield return null;
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        yield return null;
        distanceEmitter.Stop();
        if (isMine)
        {
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }
}
