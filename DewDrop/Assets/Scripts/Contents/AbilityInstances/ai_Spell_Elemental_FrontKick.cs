using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Elemental_FrontKick : AbilityInstance
{
    public SelfValidator channelValidator;
    public float duration;
    public float distance;

    public float airborneDuration;
    public float pushDistance;

    public float bonusDamage;
    public TargetValidator targetValidator;

    private Vector3 start;


    ParticleSystem whoof;
    GameObject flash;

    private bool didHit;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        didHit = false;
        whoof = transform.Find("Whoof").GetComponent<ParticleSystem>();
        flash = transform.Find("Flash").gameObject;
        if (photonView.IsMine)
        {
            Channel channel = new Channel(channelValidator, duration, false, false, false, false, null, End);
            info.owner.control.StartChanneling(channel);
        }

        whoof.Play();
        start = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += info.directionVector * distance / duration * Time.deltaTime;
        if(Vector3.Distance(transform.position, start) >= distance && photonView.IsMine)
        {
            End();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || !isAlive) return;
        Entity lv = other.GetComponent<Entity>();
        if (lv == null) return;
        if (!targetValidator.Evaluate(info.owner, lv)) return;

        lv.StartDisplacement(Displacement.ByVector(info.directionVector * pushDistance, airborneDuration, false, false, false, Ease.EaseOutSine));

        info.owner.DoBasicAttackImmediately(lv, this);
        info.owner.DoMagicDamage(lv, bonusDamage, false, this);
        Vector3 hitPos = other.ClosestPoint(transform.position);
        photonView.RPC("CreateFlash", RpcTarget.All, hitPos);
        SFXManager.CreateSFXInstance("si_Spell_Elemental_FrontKick Hit", transform.position);
        if(!didHit)
        {
            didHit = true;
            if (info.owner.control.skillSet[2] == null) return;
            trg_Spell_Elemental_DoubleKick trg = info.owner.control.skillSet[2] as trg_Spell_Elemental_DoubleKick;
            if (trg != null) trg.ResetCooldown();
        }
        
    }


    [PunRPC]
    private void CreateFlash(Vector3 location)
    {
        Quaternion rotation = Quaternion.LookRotation(location - info.owner.transform.position - info.owner.GetCenterOffset(), Vector3.up);
        
        Instantiate(flash, location, rotation, transform).GetComponent<ParticleSystem>().Play();
    }


    private void End()
    {
        if (!isAlive) return;
        Despawn();

    }

}
