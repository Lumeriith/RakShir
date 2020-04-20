using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_Breakthrough : AbilityInstance
{
    public float dashDuration = 0.5f;
    public float dashDistance = 5.5f;
    public float slowAmount = 35f;
    public float damage = 100f;
    public float duration = 4f;

    public TargetValidator targetValidator;

    private ParticleSystem main;
    private GameObject smallFire;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        transform.parent = info.owner.transform;
        transform.position = info.owner.transform.position;
        transform.rotation = info.directionQuaternion;
        main = transform.Find<ParticleSystem>("Main");
        smallFire = transform.Find("SmallFire").gameObject;
        main.Play();
        if (!isMine) return;
        info.owner.StartDisplacement(Displacement.ByVector(info.directionVector * dashDistance, dashDuration, true, true, true, Ease.Linear, Finished, Finished));
        SFXManager.CreateSFXInstance("si_Spell_Rare_Breakthrough Start", transform.position).Follow(this);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
        transform.rotation = info.owner.transform.rotation;
    }

    private void Finished()
    {
        DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
        DestroySelf();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        thing.ApplyStatusEffect(StatusEffect.Slow(source, duration, slowAmount));
        thing.ApplyStatusEffect(StatusEffect.DamageOverTime(source, duration, damage));
        photonView.RPC("RpcSmallFire", RpcTarget.All, thing.photonView.ViewID);
        SFXManager.CreateSFXInstance("si_Spell_Rare_Breakthrough Hit", thing.transform.position);
    }

    [PunRPC]
    private void RpcSmallFire(int id)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(id).GetComponent<LivingThing>();
        GameObject newFire = Instantiate(smallFire, thing.transform.position + thing.GetCenterOffset(), Quaternion.identity, thing.transform);
        newFire.GetComponent<ParticleSystem>().Play();
        newFire.AddComponent<ParticleSystemAutoDestroy>();
    }

}
