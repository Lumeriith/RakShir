using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class gem_Epic_Fissure : Gem
{
    [SerializeField]
    private float _baseDamage = 10f;
    [SerializeField]
    private float[] _damagePerFissureStack = { 0.5f, 1.0f, 1.5f };
    [SerializeField]
    private float _delay = 0.1f;

    public float delay => _delay;
    public float totalDamage { get => _baseDamage + _damagePerFissureStack[level] * _fissureStacks; }
    private int _fissureStacks = 0;

    public override void OnAbilityInstanceCreatedFromTrigger(bool isMine, AbilityInstance instance)
    {
        if(isMine) instance.OnDealMagicDamage += Fissure;
    }

    private void Fissure(InfoMagicDamage info)
    {
        info.caller.OnDealMagicDamage -= Fissure;
        CreateAbilityInstance("ai_Gem_Epic_Fissure", info.to.transform.position + info.to.GetCenterOffset(), Quaternion.identity, CastInfo.OwnerAndTarget(info.from, info.to));
        _fissureStacks++;
    }
}
