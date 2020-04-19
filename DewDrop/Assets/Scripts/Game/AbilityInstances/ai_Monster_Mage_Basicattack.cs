using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Monster_Mage_Basicattack : AbilityInstance
{
    private ParticleSystem land;
    private ParticleSystem fly;

    public TargetValidator tv;
    public float damage = 60f;
    public float rootDuration = 1.5f;
    public float distance = 7.5f;
    public float speed = 5f;

    private Vector3 startPos;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
        fly.Play();
        startPos = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (photonView.IsMine)
        {
            if(Vector3.Distance(transform.position, startPos) > distance)
            {
                fly.Stop();
                DetachChildParticleSystemsAndAutoDelete();
                DestroySelf();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing lv = other.GetComponent<LivingThing>();
        if (lv == null || !tv.Evaluate(info.owner, lv)) return;
        photonView.RPC("RpcLanded", RpcTarget.All, lv.transform.position + lv.GetCenterOffset());
        info.owner.DoMagicDamage(damage, lv, false, source);
        lv.statusEffect.ApplyStatusEffect(StatusEffect.Root(source, rootDuration));
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }


    [PunRPC]
    private void RpcLanded(Vector3 pos)
    {
        transform.position = pos;
        land.Play();
        fly.Stop();
    }
}
