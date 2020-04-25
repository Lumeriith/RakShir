using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class ai_Spell_Rare_AuraOfPain : AbilityInstance
{
    public int ticks = 8;
    public float tickInterval = 0.5f;
    public float slowAmount = 10f;
    public float damage = 20f;
    public float radius = 3f;
    public TargetValidator targetValidator;

    private ParticleSystem aura;
    private GameObject hit;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        aura = transform.Find("Aura").GetComponent<ParticleSystem>();
        hit = transform.Find("Hit").gameObject;
        transform.position = info.owner.transform.position;

        aura.Play();
        if (photonView.IsMine)
        {
            StartCoroutine(CoroutineAuraOfPain());
        }
    }


    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;

    }
    private IEnumerator CoroutineAuraOfPain()
    {
        List<LivingThing> targets;
        for(int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
            targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                SFXManager.CreateSFXInstance("si_Spell_Rare_AuraOfPain Hit", targets[j].transform.position);
                targets[j].ApplyStatusEffect(StatusEffect.Slow(tickInterval, slowAmount), this);
                info.owner.DoMagicDamage(targets[j], damage, false, this);
                photonView.RPC("RpcHit", RpcTarget.All, targets[j].photonView.ViewID);
            }
        }

        
        Despawn();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, transform);
    }
}
