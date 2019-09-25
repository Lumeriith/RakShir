using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LivingThingStatusEffect : MonoBehaviourPun
{
    List<CoreStatusEffect> coreStatusEffects;
    private LivingThing livingThing;

    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
    }

    public void ApplyCoreStatusEffect(CoreStatusEffect ce)
    {
        int uid = Random.Range(int.MinValue, int.MaxValue);
        while(RetrieveCoreStatusEffect(uid) != null) // Just in case the uid generation threads the needle:
        {
            uid = Random.Range(int.MinValue, int.MaxValue); // Reroll.
        }
        ce.uid = uid;
        photonView.RPC("RpcApplyCoreStatusEffect", RpcTarget.AllViaServer, uid, ce.caster.photonView.ViewID, (byte)ce.type, ce.duration, ce.parameter);
    }

    public void RemoveCoreStatusEffect(CoreStatusEffect ce)
    {
        photonView.RPC("RpcRemoveCoreStatusEffect", RpcTarget.AllViaServer, ce.uid);
    }

    public bool IsAffectedBy(CoreStatusEffectType type) // TODO: Cache this!
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.type == type) return true;
        }
        return false;
    }

    public CoreStatusEffect RetrieveCoreStatusEffect(int uid)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.uid == uid) return ce;
        }
        return null;
    }


    private void FixedUpdate()
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(IsAffectedBy(CoreStatusEffectType.Stasis))
            ce.duration -= Time.deltaTime;
        }
    }

    [PunRPC]
    public void RpcApplyCoreStatusEffect(int uid, int casterViewID, byte type, float duration, object parameter)
    {
        LivingThing caster = PhotonNetwork.GetPhotonView(casterViewID).GetComponent<LivingThing>();
        LivingThing owner = livingThing;

        CoreStatusEffect ce = new CoreStatusEffect(caster, owner, (CoreStatusEffectType)type, duration, parameter);
        coreStatusEffects.Add(ce);
    }

    [PunRPC]
    public void RpcRemoveCoreStatusEffect(int uid)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(ce.uid == uid)
            {
                coreStatusEffects.Remove(ce); // Fix something...
                break;
            }
        }
    }

    

}
