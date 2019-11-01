using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_BurningGrounds : AbilityInstance
{
    private GameObject hit;
    public int ticks = 8;
    public float tickInterval = 0.5f;
    public float damage = 30f;
    public float range = 3f;
    public TargetValidator targetValidator;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").gameObject;
        transform.Find("Burn").GetComponent<ParticleSystem>().Play();
        if (photonView.IsMine) StartCoroutine(CoroutineBurningGrounds());
    }

    private IEnumerator CoroutineBurningGrounds()
    {
        List<LivingThing> targets;
        for(int i = 0; i < ticks; i++)
        {
            targets = info.owner.GetAllTargetsInRange(transform.position, range, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                info.owner.DoMagicDamage(damage, targets[j]);
                photonView.RPC("RpcHit", RpcTarget.All, targets[j].photonView.ViewID);
            }
            yield return new WaitForSeconds(tickInterval);
        }
        DetachChildParticleSystemsAndAutoDelete();
        DestroySelf();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        Transform target = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(hit, target.transform.position + target.GetComponent<LivingThing>().GetCenterOffset(), Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }

}
