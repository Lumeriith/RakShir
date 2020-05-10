using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyBehaviour : EntityBehaviour
{
    private const float LeashImmediatelyRange = 5f;
    private const float LeashRange = 0.25f;
    private const float LeashTime = 4.5f;

    private Vector3 _originalPosition;
    private float _elapsedLeashTime = 0f;

    private void Start()
    {
        _originalPosition = transform.position;

        entity.OnDeath += (_)=>
        {
            if (!entity.isMine) return;
            entity.Revive();
            entity.ApplyStatusEffect(StatusEffect.Invulnerable(1.5f), null);
            entity.ApplyStatusEffect(StatusEffect.HealOverTime(1.5f, entity.maximumHealth, true), null);
        };
    }

    private void Update()
    {
        if (!entity.isMine) return;

        if(Vector3.Distance(transform.position, _originalPosition) > LeashRange)
        {
            _elapsedLeashTime += Time.deltaTime;
        }

        if(_elapsedLeashTime > LeashTime || Vector3.Distance(transform.position,_originalPosition) > LeashImmediatelyRange)
        {
            _elapsedLeashTime = 0f;
            entity.Teleport(_originalPosition);
        }
    }
}
