using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ai_Spell_Rare_Ambush : AbilityInstance
{
    [SerializeField]
    private float _animationDuration = 0.5f;
    [SerializeField]
    private float _distanceBehindTarget = 1.25f;
    [SerializeField]
    private float _dashDuration = 0.25f;
    [SerializeField]
    private float _damage = 55f;
    [SerializeField]
    private float _resetCooldownTime = 1f;
    [SerializeField]
    private TargetValidator _validator;

    private List<Entity> _affectedTargets = new List<Entity>();
    private GameObject _hitEffect;

    protected override void OnCreate(CastInfo info, object[] data)
    {
        _hitEffect = transform.Find("Hit Effect").gameObject;
        if (isMine)
        {
            Vector3 delta = info.target.transform.position - info.owner.transform.position;
            delta += delta.normalized * _distanceBehindTarget;
            info.owner.PlayCustomAnimation("Rare - Ambush", _animationDuration);
            info.owner.StartDisplacement(Displacement.ByVector(delta, _dashDuration, true, true, true, Ease.EaseInBack, WaitForDeadTargets, WaitForDeadTargets));
        }
    }

    protected override void AliveUpdate()
    {
        transform.position = info.owner.transform.position + info.owner.GetCenterOffset();
        transform.rotation = info.owner.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMine) return;
        Entity entity = other.GetComponent<Entity>();
        if (entity == null || !_validator.Evaluate(info.owner, entity) && _affectedTargets.Contains(entity)) return;
        _affectedTargets.Add(entity);
        info.owner.DoMagicDamage(entity, _damage, false, this);
        SFXManager.CreateSFXInstance("si_Spell_Rare_Ambush Hit", entity.transform.position);
        photonView.RPC(nameof(RpcAmbushHit), RpcTarget.All, entity.photonView.ViewID);
    }

    [PunRPC]
    private void RpcAmbushHit(int hitTargetViewID)
    {
        Entity hitTarget = PhotonNetwork.GetPhotonView(hitTargetViewID).GetComponent<Entity>();
        GameObject newEffect = Instantiate(_hitEffect, hitTarget.transform.position, Quaternion.identity);
        newEffect.GetComponent<ParticleSystem>().Play();
        newEffect.AddComponent<ParticleSystemAutoDestroy>();
    }

    private void WaitForDeadTargets()
    {
        StartCoroutine(CoroutineWaitForDeadTargets());
    }

    private IEnumerator CoroutineWaitForDeadTargets()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _resetCooldownTime)
        {
            elapsedTime += Time.deltaTime;
            for(int i = 0; i < _affectedTargets.Count; i++)
            {
                if (_affectedTargets[i].IsDead())
                {

                }
            }
            yield return null;
        }
        Despawn(DespawnBehaviour.StopAndWaitForParticleSystems);
    }
}
