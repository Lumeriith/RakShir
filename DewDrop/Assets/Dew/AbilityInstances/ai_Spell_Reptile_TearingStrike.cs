using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Reptile_TearingStrike : AbilityInstance
{
    public SelfValidator channelValidator;
    public float duration;
    public float distance;

    public float slowDuration;
    public float slowAmount;

    public float damage = 40f;
    public TargetValidator targetValidator;

    private Vector3 start;

    public float postChannelTime = 0.25f;

    private ParticleSystem _whoof;
    private GameObject _flash;

    private bool didHit;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        didHit = false;
        _whoof = transform.Find("Whoof").GetComponent<ParticleSystem>();
        _flash = transform.Find("Flash").gameObject;
        if (photonView.IsMine)
        {
            Channel channel = new Channel(channelValidator, duration + postChannelTime, false, false, false, false, null, EndStrike);
            info.owner.control.StartChanneling(channel);
        }

        _whoof.Play();
        start = transform.position;
    }

    protected override void AliveUpdate()
    {
        if(isMine && Vector3.Distance(transform.position, start) > distance)
        {
            EndStrike();
        }
        transform.position += info.directionVector * distance / duration * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || !isAlive) return;
        Entity lv = other.GetComponent<Entity>();
        if (lv == null) return;
        if (!targetValidator.Evaluate(info.owner, lv)) return;

        lv.ApplyStatusEffect(StatusEffect.Slow(slowDuration, slowAmount), this);

        info.owner.DoMagicDamage(lv, damage, false, this);
        Vector3 hitPos = other.ClosestPoint(transform.position);
        photonView.RPC(nameof(CreateFlash), RpcTarget.All, hitPos);
        SFXManager.CreateSFXInstance("si_Spell_Reptile_TearingStrike Hit", transform.position);
        if(!didHit)
        {
            didHit = true;
            if (info.owner.control.skillSet[2] == null) return;
            if (info.owner.control.skillSet[2] is trg_Spell_Reptile_Swipe trigger) trigger.ResetCooldown();
        }
        
    }


    [PunRPC]
    private void CreateFlash(Vector3 location)
    {
        Quaternion rotation = Quaternion.LookRotation(location - info.owner.transform.position - info.owner.GetCenterOffset(), Vector3.up);
        
        Instantiate(_flash, location, rotation, transform).GetComponent<ParticleSystem>().Play();
    }


    private void EndStrike()
    {
        if (!isAlive) return;
        photonView.RPC(nameof(RpcStopWhoof), RpcTarget.All);
        Despawn();
    }

    [PunRPC]
    private void RpcStopWhoof()
    {
        _whoof.Stop();
    }

}
