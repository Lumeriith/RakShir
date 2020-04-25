using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ai_Monster_Mage_Lightning : AbilityInstance
{
    public float radius = 1.5f;
    public int ticks = 12;
    public float interval = 0.25f;
    public float damage = 7.5f;

    public TargetValidator tv;

    private GameObject hit;
    private ParticleSystem explosion;

    protected override void OnCreate(CastInfo castInfo, object[] data)
    {
        hit = transform.Find("Hit").gameObject;
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
        explosion.Play();
        if (photonView.IsMine) StartCoroutine(CoroutineMagic());
    }

    IEnumerator CoroutineMagic()
    {
        List<Entity> lvs;
        for (int i = 0; i < ticks; i++)
        {
            lvs = info.owner.GetAllTargetsInRange(transform.position, radius, tv);
            for (int j = 0; j < lvs.Count; j++)
            {
                info.owner.DoMagicDamage(lvs[j], damage, false, this);
                photonView.RPC("RpcHit", RpcTarget.All, lvs[j].transform.position + lvs[j].GetCenterOffset());
            }
            yield return new WaitForSeconds(interval);
        }
        
        Despawn();
    }

    [PunRPC]
    private void RpcHit(Vector3 pos)
    {
        GameObject gobj = Instantiate(hit, pos, Quaternion.identity);
        gobj.GetComponent<ParticleSystem>().Play();
        gobj.AddComponent<ParticleSystemAutoDestroy>();
    }




}
