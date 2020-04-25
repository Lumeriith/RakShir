using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_HealingCircle : AbilityInstance
{
    public TargetValidator targetValidator;
    public int ticks = 12;
    public float tickInterval = 0.5f;
    public float healAmount = 30f;
    public float radius = 3.5f;

    private ParticleSystem circle;
    private GameObject hit;

    private SFXInstance loopSFX;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        circle = transform.Find("Circle").GetComponent<ParticleSystem>();
        circle.Play();
        hit = transform.Find("Hit").gameObject;
        if (!photonView.IsMine) return;
        SFXManager.CreateSFXInstance("si_Spell_Rare_HealingCircle Start", transform.position);
        loopSFX = SFXManager.CreateSFXInstance("si_Spell_Rare_HealingCircle Loop", transform.position);
        StartCoroutine(CoroutineHeal());
    }

    private IEnumerator CoroutineHeal()
    {
        List<LivingThing> targets;
        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
            targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                info.owner.DoHeal(targets[j], healAmount, false, reference);
                photonView.RPC("RpcHit", RpcTarget.All, targets[j].photonView.ViewID);
            }
        }
        yield return new WaitForSeconds(0.1f);
        loopSFX.DestroyFadingOut(1f);
        Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
        Despawn();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, transform);
    }
}
