using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Ignite : AbilityInstance
{
    CastInfo info;
    private Vector3 centerOffset;
    
    public float speed;
    public float explosionTime;
    public float explosionRadius;
    public float damage;
    public TargetValidator targetValidator;
    public CoreStatusEffectType ccType;
    public float ccDuration;

    private float elapsedTimeAfterLatched = 0;

    private bool isLatched = false;

    private ParticleSystem beforeLatch;
    private ParticleSystem latched;
    private ParticleSystem explode;

    private void Awake()
    {
        beforeLatch = transform.Find("BeforeLatch").GetComponent<ParticleSystem>();
        latched = transform.Find("Latched").GetComponent<ParticleSystem>();
        explode = transform.Find("Explode").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        info = castInfo;
        centerOffset = castInfo.target.GetCenterOffset();
        beforeLatch.Play();
    }

    protected override void AliveUpdate()
    {
        
        if (!isLatched)
        {
            transform.position = Vector3.MoveTowards(transform.position, info.target.transform.position + centerOffset, speed * Time.deltaTime);

            if(transform.position == info.target.transform.position + centerOffset)
            {
                isLatched = true;
                beforeLatch.Stop();
                latched.Play();
            }

        }
        else
        {
            transform.position = info.target.transform.position + centerOffset;
            if (photonView.IsMine)
            {
                elapsedTimeAfterLatched += Time.deltaTime;
                if (elapsedTimeAfterLatched > explosionTime)
                {
                    photonView.RPC("ExplodeEffect", RpcTarget.AllViaServer);

                    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("LivingThing"));

                    foreach (Collider collider in colliders)
                    {
                        LivingThing lv = collider.GetComponent<LivingThing>();
                        if (lv == null) continue;
                        if (targetValidator.Evaluate(info.owner, lv))
                        {
                            CoreStatusEffect cc = new CoreStatusEffect(info.owner, ccType, ccDuration);

                            lv.statusEffect.ApplyCoreStatusEffect(cc);
                            info.owner.DoMagicDamage(damage, lv);

                        }
                    }

                    DetachChildParticleSystemsAndAutoDelete();
                    DestroySelf();

                }
            }
        }


    }

    [PunRPC]
    private void ExplodeEffect()
    {
        explode.Play();
        latched.Stop();
    }
}
