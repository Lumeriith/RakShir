using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Spell_Rare_DarkEnergy : AbilityInstance
{
    public float radius = 3f;
    public float silenceDuration = 1.5f;
    public TargetValidator targetValidator;
    private GameObject hit;
    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").gameObject;
        if (!photonView.IsMine) return;
        info.owner.statusEffect.CleanseAllHarmfulStatusEffects();
        List<LivingThing> targets = info.owner.GetAllTargetsInRange(transform.position, radius, targetValidator);
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].ApplyStatusEffect(StatusEffect.Silence(info.owner, silenceDuration));
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
        }
        DestroySelf(5f);
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position;
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        Transform t = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(hit, t.position, Quaternion.identity, t).AddComponent<ParticleSystemAutoDestroy>();
    }
}
