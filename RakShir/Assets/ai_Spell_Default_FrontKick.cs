using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Default_FrontKick : AbilityInstance
{
    public float duration;
    public float distance;

    public float airborneDuration;
    public float pushDistance;

    public float bonusDamage;
    public TargetValidator targetValidator;

    private Vector3 start;

    CastInfo info;

    ParticleSystem whoof;
    GameObject flash;


    private void Awake()
    {
        whoof = transform.Find("Whoof").GetComponent<ParticleSystem>();
        flash = transform.Find("Flash").gameObject;
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        info = castInfo;
        if (photonView.IsMine)
        {
            info.owner.control.StartChanneling(duration, null, End);
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

    private bool didHit = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || !isAlive) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null) return;
        if (!targetValidator.Evaluate(info.owner, lv)) return;

        lv.AirborneForDuration(lv.transform.position + info.directionVector * pushDistance, airborneDuration);

        info.owner.DoBasicAttackImmediately(lv);
        info.owner.DoMagicDamage(bonusDamage, lv);
        Vector3 hitPos = other.ClosestPoint(transform.position);
        photonView.RPC("CreateFlash", RpcTarget.All, hitPos);
        if(!didHit)
        {
            didHit = true;
            if (info.owner.control.keybindings[1] == null) return;
            trg_Spell_Default_DoubleKick trg = info.owner.control.keybindings[1] as trg_Spell_Default_DoubleKick;
            if (trg != null) trg.ResetCooldown();
        }
        
    }


    [PunRPC]
    private void CreateFlash(Vector3 location)
    {
        Instantiate(flash, location, Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }


    private void End()
    {
        if (!isAlive) return;
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();

    }

}
