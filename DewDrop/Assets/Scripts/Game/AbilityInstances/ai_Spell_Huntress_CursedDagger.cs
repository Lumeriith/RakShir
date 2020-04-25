using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Huntress_CursedDagger : AbilityInstance
{
    private ParticleSystem linger;
    private ParticleSystem curse;
    private GameObject dagger;

    public float flySpeed = 10f;
    public float daggerRotationSpeed = 900f;
    public float curseInterval = 2.5f;
    public float maximumDuration = 3.5f;


    private float lingerTime;
    private float remainingDuration;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        linger = transform.Find("Linger").GetComponent<ParticleSystem>();
        curse = transform.Find("Curse").GetComponent<ParticleSystem>();
        dagger = transform.Find("Dagger").gameObject;
        dagger.GetComponent<ParticleSystem>().Play();
        
    }

    private void OwnerBasicAttackHit(InfoBasicAttackHit info)
    {
        if (info.to != this.info.target) return;
        remainingDuration = maximumDuration;
        photonView.RPC("RpcResetLinger", RpcTarget.All);
    }

    protected override void AliveUpdate()
    {
        if (dagger.activeSelf)
        {

            dagger.transform.Rotate(dagger.transform.right, daggerRotationSpeed * Time.deltaTime);
            transform.LookAt(info.target.transform.position + info.target.GetCenterOffset());
            transform.position = Vector3.MoveTowards(transform.position, info.target.transform.position + info.target.GetCenterOffset(), flySpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position, info.target.transform.position+info.target.GetCenterOffset()) < .3f)
            {
                dagger.transform.Find("Darkness").parent = transform;
                transform.Find("Darkness").GetComponent<ParticleSystem>().Stop();
                dagger.SetActive(false);
                linger.Play();
                lingerTime = 0f;
                remainingDuration = maximumDuration;
                if (photonView.IsMine) info.owner.OnDoBasicAttackHit += OwnerBasicAttackHit;
            }
        }
        else 
        {
            transform.position = info.target.transform.position + info.target.GetCenterOffset();
            if (photonView.IsMine)
            {
                lingerTime += Time.deltaTime;
                remainingDuration -= Time.deltaTime;

                if (lingerTime > curseInterval)
                {
                    lingerTime = 0f;
                    info.target.statusEffect.ApplyStatusEffect(StatusEffect.Silence(1f), this);
                    info.owner.DoMagicDamage(info.target, 75f, false, this);
                    photonView.RPC("RpcCurseEffect", RpcTarget.All);
                }

                if(remainingDuration < 0)
                {
                    info.owner.OnDoBasicAttackHit -= OwnerBasicAttackHit;
                    Despawn();
                }
                
            }
        }
    }

    [PunRPC]
    private void RpcCurseEffect()
    {
        curse.Play();
    }

    [PunRPC]
    private void RpcResetLinger()
    {
        linger.Play();
    }


}
