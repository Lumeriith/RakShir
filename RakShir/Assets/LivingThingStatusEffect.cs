using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LivingThingStatusEffect : MonoBehaviourPun
{
    List<CoreStatusEffect> coreStatusEffects = new List<CoreStatusEffect>();
    private LivingThing livingThing;

    private SelfValidator sv_CanHaveHarmfulCoreStatusEffects = (SelfValidator)SelfValidator.sv_CanHaveHarmfulCoreStatusEffects.Clone();


    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        sv_CanHaveHarmfulCoreStatusEffects.SetSelf(livingThing);
    }

    public void ApplyCoreStatusEffect(CoreStatusEffect ce)
    {
        int uid = Random.Range(int.MinValue, int.MaxValue);
        while(RetrieveCoreStatusEffect(uid) != null) // Just in case the uid generation threads the needle:
        {
            uid = Random.Range(int.MinValue, int.MaxValue); // Reroll.
        }
        ce.uid = uid;
        ce.owner = livingThing;
        coreStatusEffects.Add(ce);
        photonView.RPC("RpcApplyCoreStatusEffect", RpcTarget.Others, uid, ce.caster.photonView.ViewID, (byte)ce.type, ce.duration, ce.parameter);
    }

    public void CleanseCoreStatusEffect(CoreStatusEffectType type)
    {
        photonView.RPC("RpcCleanseCoreStatusEffect", RpcTarget.AllViaServer, (byte)type);
    }

    public void RemoveCoreStatusEffect(CoreStatusEffect ce)
    {
        if (ce.isAboutToBeDestroyed) return;
        ce.isAboutToBeDestroyed = true;
        photonView.RPC("RpcRemoveCoreStatusEffect", RpcTarget.AllViaServer, ce.uid);
    }

    public void AddDurationToCoreStatusEffect(CoreStatusEffect ce, float duration)
    {
        photonView.RPC("RpcAddDurationToCoreStatusEffect", RpcTarget.AllViaServer, ce.uid, duration);
    }

    public void SetDurationOfCoreStatusEffect(CoreStatusEffect ce, float duration)
    {
        photonView.RPC("RpcSetDurationOfCoreStatusEffect", RpcTarget.AllViaServer, ce.uid, duration);
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
        if (IsAffectedBy(CoreStatusEffectType.Stasis)) return;
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.isAboutToBeDestroyed) continue;
            ce.duration = Mathf.MoveTowards(ce.duration, 0, Time.deltaTime);
            if (photonView.IsMine)
            {
                if(ce.duration <= 0 || (!sv_CanHaveHarmfulCoreStatusEffects.IsValid() && ce.IsHarmful()))
                {
                    RemoveCoreStatusEffect(ce);
                }
            }

        }
    }

    [PunRPC]
    public void RpcApplyCoreStatusEffect(int uid, int casterViewID, byte type, float duration, object parameter)
    {
        LivingThing caster = PhotonNetwork.GetPhotonView(casterViewID).GetComponent<LivingThing>();
        LivingThing owner = livingThing;

        CoreStatusEffect ce = new CoreStatusEffect(caster, (CoreStatusEffectType)type, duration, parameter);
        ce.owner = livingThing;
        ce.uid = uid;
        coreStatusEffects.Add(ce);
    }

    [PunRPC]
    public void RpcRemoveCoreStatusEffect(int uid)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(ce.uid == uid)
            {
                ce.duration = 0;
                ce.isAboutToBeDestroyed = true;
                coreStatusEffects.Remove(ce); // Fix something...
                break;
            }
        }
    }

    [PunRPC]
    public void RpcAddDurationToCoreStatusEffect(int uid, float duration)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if (ce.uid == uid)
            {
                ce.duration += duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcSetDurationOfCoreStatusEffect(int uid, float duration)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(ce.uid == uid)
            {
                ce.duration = duration;
                break;
            }
        }
    }

    [PunRPC]
    public void RpcCleanseCoreStatusEffect(byte type)
    {
        foreach(CoreStatusEffect ce in coreStatusEffects)
        {
            if(ce.type == (CoreStatusEffectType)type)
            {
                ce.duration = 0;
                ce.isAboutToBeDestroyed = true;
                coreStatusEffects.Remove(ce);
            }
        }
    }
}
