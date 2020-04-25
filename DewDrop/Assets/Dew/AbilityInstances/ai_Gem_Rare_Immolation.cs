using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Gem_Rare_Immolation : AbilityInstance
{
    

    private gem_Rare_Immolation immolation;

    private ParticleSystem main;
    private GameObject hit;
    private ParticleSystem circle;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.position = info.owner.transform.position;
        transform.parent = info.owner.transform;
        immolation = (gem_Rare_Immolation)gem;
        main = transform.Find<ParticleSystem>("Main");
        hit = transform.Find("Hit").gameObject;
        circle = transform.Find<ParticleSystem>("Circle");
        main.Play();
        if (isMine) StartCoroutine(CoroutineImmolation());
    }

    protected override void AliveUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    IEnumerator CoroutineImmolation()
    {
        List<LivingThing> targets;
        while(immolation.timeLeft > 0f)
        {
            photonView.RPC("RpcCircleEffect", RpcTarget.All);
            targets = info.owner.GetAllTargetsInRange(transform.position, immolation.radius, immolation.burnableTargets);
            SFXManager.CreateSFXInstance("si_Gem_Rare_Immolation", transform.position);
            for (int i = 0; i < targets.Count; i++)
            {
                info.owner.DoMagicDamage(targets[i], immolation.damagePerTick[immolation.level], false, reference);
                photonView.RPC("RpcCreateHitEffect", RpcTarget.All, targets[i].photonView.ViewID);
                SFXManager.CreateSFXInstance("si_Gem_Rare_Immolation Hit", targets[i].transform.position);
            }
            yield return new WaitForSeconds(immolation.tickTime);
            
        }
        Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
    }

    [PunRPC]
    private void RpcCircleEffect()
    {
        circle.Play();
    }

    [PunRPC]
    private void RpcCreateHitEffect(int viewID)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        if (view == null) return;
        LivingThing thing = view.GetComponent<LivingThing>();
        GameObject newHit = Instantiate(hit, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, thing.transform.parent);
        newHit.GetComponent<ParticleSystem>().Play();
        newHit.AddComponent<ParticleSystemAutoDestroy>();
    }
}
