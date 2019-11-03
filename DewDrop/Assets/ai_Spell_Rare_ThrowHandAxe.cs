using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_ThrowHandAxe : AbilityInstance
{
    public float flyDuration = 2.5f;
    public float distance = 10f;
    public AnimationCurve movementCurve;

    public TargetValidator targetValidator;

    public float damage = 10f;
    public float slowAmount = 30f;
    public float slowDuration = 4f;

    public float modelAngularSpeed = 700f;

    private Vector3 start;
    private Vector3 destination;

    private ParticleSystem hit;
    private ParticleSystem fly;
    private Transform model;
    private float elapsedTime = 0f;
    private new Collider collider;
    private bool isComingBack = false;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").GetComponent<ParticleSystem>();
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        collider = GetComponent<Collider>();
        model = transform.Find("Model");
        fly.Play();
        start = transform.position;
        destination = transform.position + info.directionVector * distance;
    }

    protected override void AliveUpdate()
    {
        elapsedTime += Time.deltaTime;
        model.Rotate(Vector3.up, modelAngularSpeed * Time.deltaTime, Space.World);
        if (elapsedTime < flyDuration / 2f)
        {
            transform.position = Vector3.Lerp(start, destination, movementCurve.Evaluate(elapsedTime / flyDuration));
        }
        else
        {
            if (!isComingBack)
            {
                collider.enabled = false;
                collider.enabled = true;
                isComingBack = true;
            }
            transform.position = Vector3.Lerp(info.owner.transform.position, destination, movementCurve.Evaluate(elapsedTime / flyDuration));
        }

        if(elapsedTime > flyDuration && photonView.IsMine)
        {
            DetachChildParticleSystemsAndAutoDelete(DetachBehaviour.StopEmitting);
            DestroySelf();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing thing = other.GetComponent<LivingThing>();
        if (thing == null || !targetValidator.Evaluate(info.owner, thing)) return;
        thing.ApplyStatusEffect(StatusEffect.Slow(info.owner, slowDuration, slowAmount));
        info.owner.DoMagicDamage(damage, thing);
        photonView.RPC("RpcHit", RpcTarget.All, thing.photonView.ViewID);
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }
}
