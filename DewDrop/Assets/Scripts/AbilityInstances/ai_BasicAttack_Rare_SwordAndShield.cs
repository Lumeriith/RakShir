using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_BasicAttack_Rare_SwordAndShield : AbilityInstance
{
    public TargetValidator validator;
    public float splashRadius = 1.5f;
    public int maxTargets = 2;
    private GameObject hit;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").gameObject;

        if (!photonView.IsMine) return;
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(info.target.transform.position, splashRadius, validator);
        for(int i = 0; i < maxTargets; i++)
        {
            if (targets.Count <= i) break;
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
            info.owner.DoBasicAttackImmediately(targets[i]);
            SFXManager.CreateSFXInstance("si_BasicAttack_Rare_SwordAndShield " + Random.Range(0, 3), targets[i].transform.position);
        }
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }


    [PunRPC]
    private void RpcHit(int viewID)
    {
        LivingThing thing = PhotonNetwork.GetPhotonView(viewID).GetComponent<LivingThing>();
        Instantiate(hit, thing.transform.position + thing.GetRandomOffset(), Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }

}
