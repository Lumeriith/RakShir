using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Gem_Rare_Soul : AbilityInstance
{
    private bool hasSucceeded = false;

    private gem_Rare_Soul soul;
    private ParticleSystem scar;
    private ParticleSystem orb;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        soul = (gem_Rare_Soul)gem;
        scar = transform.Find<ParticleSystem>("Scar");
        orb = transform.Find<ParticleSystem>("Orb");
        scar.Play();
        if (!isMine) return;
        SFXManager.CreateSFXInstance("si_Gem_Rare_Soul Latch", transform.position);
        StartCoroutine("CoroutineCheck");
    }

    protected override void AliveUpdate()
    {
        if(!hasSucceeded) transform.position = info.target.transform.position + info.target.GetCenterOffset();
        else
        {
            transform.rotation = Quaternion.LookRotation(info.owner.transform.position + info.owner.GetCenterOffset() - transform.position, Vector3.up);
            transform.position = Vector3.MoveTowards(transform.position, info.owner.transform.position + info.owner.GetCenterOffset(), soul.orbSpeed * Time.deltaTime);
            if(isMine && Vector3.Distance(transform.position, info.owner.transform.position + info.owner.GetCenterOffset()) < .15f)
            {
                SFXManager.CreateSFXInstance("si_Gem_Rare_Soul Eat", transform.position);
                photonView.RPC("RpcIncreaseHealth", RpcTarget.All);
                Despawn(info.owner, DespawnBehaviour.StopAndWaitForParticleSystems);
                Despawn();
            }
        }
    }

    [PunRPC]
    private void RpcIncreaseHealth()
    {
        if (soul.increasedHealth + soul.healthPerKill > soul.healthBonusLimit[soul.level])
        {
            info.owner.stat.baseMaximumHealth += soul.healthBonusLimit[soul.level] - soul.increasedHealth;
            soul.increasedHealth = soul.healthBonusLimit[soul.level];
        }
        else
        {
            info.owner.stat.baseMaximumHealth += soul.healthPerKill;
            soul.increasedHealth += soul.healthPerKill;
        }
    }

    private IEnumerator CoroutineCheck()
    {
        int index;

        while (true)
        {
            yield return new WaitForSeconds(0.10f);
            index = soul.damagedTargets.IndexOf(info.target);
            if (info.target.IsDead())
            {
                SFXManager.CreateSFXInstance("si_Gem_Rare_Soul Success", transform.position);
                photonView.RPC("RpcSuccess", RpcTarget.All);
                break;
            }
            else if (index == -1 || Time.time - soul.damagedTimes[index] > soul.scarDuration)
            {
                Despawn(info.target, DespawnBehaviour.StopAndWaitForParticleSystems);
                break;
            }
            
        }
        if(index != -1)
        {
            soul.damagedTargets.RemoveAt(index);
            soul.damagedTimes.RemoveAt(index);
        }
    }

    [PunRPC]
    private void RpcSuccess()
    {
        hasSucceeded = true;
        transform.rotation = Quaternion.LookRotation(info.owner.transform.position + info.owner.GetCenterOffset() - transform.position, Vector3.up);
        scar.Stop();
        orb.Play();
    }

}
