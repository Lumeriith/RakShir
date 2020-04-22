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
    public float slowDuration = 1.5f;
    public float slowAmount = 35f;
    public TargetValidator targetValidator;

    private SFXInstance loopSFX;
    private List<LivingThing> affectedTargets = new List<LivingThing>();

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").gameObject;
        transform.Find("Burn").GetComponent<ParticleSystem>().Play();
        if (photonView.IsMine)
        {
            SFXManager.CreateSFXInstance("si_Spell_Rare_BurningGrounds Start", transform.position);
            loopSFX = SFXManager.CreateSFXInstance("si_Spell_Rare_BurningGrounds Loop", transform.position);
            StartCoroutine(CoroutineBurningGrounds());
        }
    }

    private IEnumerator CoroutineBurningGrounds()
    {
        List<LivingThing> targets;
        for(int i = 0; i < ticks; i++)
        {
            targets = info.owner.GetAllTargetsInRange(transform.position, range, targetValidator);
            for(int j = 0; j < targets.Count; j++)
            {
                if (!affectedTargets.Contains(targets[j]))
                {
                    affectedTargets.Add(targets[j]);
                    targets[j].ApplyStatusEffect(StatusEffect.Slow(source, slowDuration, slowAmount));
                }
                info.owner.DoMagicDamage(damage, targets[j], false, source);
                photonView.RPC("RpcHit", RpcTarget.All, targets[j].photonView.ViewID);
                SFXManager.CreateSFXInstance("si_Spell_Rare_BurningGrounds Hit", transform.position);
                
            }
            yield return new WaitForSeconds(tickInterval);
        }
        loopSFX.DestroyFadingOut(1f);
        DetachChildParticleSystemsAndAutoDelete();
        Despawn();
    }

    [PunRPC]
    private void RpcHit(int viewID)
    {
        Transform target = PhotonNetwork.GetPhotonView(viewID).transform;
        Instantiate(hit, target.transform.position + target.GetComponent<LivingThing>().GetCenterOffset(), Quaternion.identity, transform).GetComponent<ParticleSystem>().Play();
    }

}
