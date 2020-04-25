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
            targets[i].ApplyStatusEffect(StatusEffect.Silence(silenceDuration), reference);
            photonView.RPC("RpcHit", RpcTarget.All, targets[i].photonView.ViewID);
        }
        Despawn(info.owner, AttachBehaviour.IgnoreRotation);
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        Transform t = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(hit, t.position, Quaternion.identity, t).AddComponent<ParticleSystemAutoDestroy>();
    }
}
