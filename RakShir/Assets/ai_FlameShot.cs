using Photon.Pun;
using UnityEngine;
public class ai_FlameShot : AbilityInstance
{
    CastInfo info;
    public float damage;
    public float speed;
    public float distance;

    private Vector3 startPosition;

    private ParticleSystem fly;
    private ParticleSystem land;

    public TargetValidator targetValidator;

    private void Awake()
    {
        fly = transform.Find("Fly").GetComponent<ParticleSystem>();
        land = transform.Find("Land").GetComponent<ParticleSystem>();
    }

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        info = castInfo;
        fly.Play();
        startPosition = transform.position;
    }

    protected override void AliveUpdate()
    {
        transform.position += info.directionVector * speed * Time.deltaTime;
        if(Vector3.Distance(startPosition, transform.position) >= distance)
        {
            fly.Stop();
            if (photonView.IsMine)
            {
                DetachChildParticleSystemsAndAutoDelete();
                DestroySelf();
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        LivingThing target = other.GetComponent<LivingThing>();
        if (target == null) return;
        if(targetValidator.Evaluate(info.owner, target))
        {
            info.owner.DoMagicDamage(50, target);
            photonView.RPC("RpcExplode", RpcTarget.All);
            DetachChildParticleSystemsAndAutoDelete();
            DestroySelf();
        }
    }

    [PunRPC]
    private void RpcExplode()
    {
        fly.Stop();
        land.Play();
    }
}
