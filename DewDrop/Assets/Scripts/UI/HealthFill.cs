using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthFill : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private Image _normalFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _upcomingHealFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _positiveDeltaFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _negativeDeltaFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _shieldFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _upcomingDamageFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _healthText;

    private Entity _target;

    private float _previousHealthUnit;
    private float _previousHealthUnitVelocity;

    [SerializeField]
    private float _deltaSmoothTime = 0.2f;
    [SerializeField]
    private float _deltaNormalizedLerpSpeed = 0.05f;

    private void Awake()
    {
        _normalFill.material = new Material(_normalFill.material);
        _upcomingHealFill.material = new Material(_upcomingHealFill.material);
        _positiveDeltaFill.material = new Material(_positiveDeltaFill.material);
        _negativeDeltaFill.material = new Material(_negativeDeltaFill.material);
        _shieldFill.material = new Material(_shieldFill.material);
        _upcomingDamageFill.material = new Material(_upcomingDamageFill.material);
    }

    public void Setup(Entity target)
    {
        _target = target;
        if(target != null) UpdateHealthFills();
    }

    private void Update()
    {
        if(_target != null) UpdateHealthFills();
    }

    private void UpdateHealthFills()
    {
        float maxUnit = Mathf.Max(_target.stat.finalMaximumHealth, _target.stat.currentHealth + _target.statusEffect.status.shield);
        float healthUnit = _target.stat.currentHealth;
        float shieldUnit = _target.statusEffect.status.shield;

        _previousHealthUnit = Mathf.SmoothDamp(_previousHealthUnit, healthUnit, ref _previousHealthUnitVelocity, _deltaSmoothTime);
        _previousHealthUnit = Mathf.MoveTowards(_previousHealthUnit, healthUnit, _deltaNormalizedLerpSpeed * maxUnit * Time.deltaTime);

        float deltaUnit = healthUnit - _previousHealthUnit;

        float upcomingHealUnit = _target.statusEffect.status.healOverTime;
        float upcomingDamageUnit = _target.statusEffect.status.damageOverTime;



        SetClip(_normalFill, 0f, healthUnit / maxUnit);
        SetClip(_shieldFill, healthUnit / maxUnit, (healthUnit + shieldUnit) / maxUnit);

        if(upcomingDamageUnit < shieldUnit)
        {
            SetClip(_upcomingDamageFill, (healthUnit + shieldUnit - upcomingDamageUnit) / maxUnit, (healthUnit + shieldUnit) / maxUnit);
            SetClip(_upcomingHealFill, (healthUnit + shieldUnit) / maxUnit, (healthUnit + shieldUnit + upcomingHealUnit) / maxUnit);
        }
        else
        {
            if(upcomingDamageUnit - shieldUnit > upcomingHealUnit)
            {
                SetClip(_upcomingDamageFill, (healthUnit + shieldUnit - upcomingDamageUnit + upcomingHealUnit) / maxUnit, (healthUnit + shieldUnit) / maxUnit);
                SetClip(_upcomingHealFill, 0f, 0f);
            }
            else
            {
                SetClip(_upcomingDamageFill, 0f, 0f);
                SetClip(_upcomingHealFill, (healthUnit + shieldUnit) / maxUnit, (healthUnit + shieldUnit + upcomingHealUnit - upcomingDamageUnit) / maxUnit);
            }
        }

        if(deltaUnit > 0)
        {
            SetClip(_positiveDeltaFill, (healthUnit - deltaUnit) / maxUnit, healthUnit / maxUnit);
            SetClip(_negativeDeltaFill, 0f, 0f);
        }
        else
        {
            SetClip(_positiveDeltaFill, 0f, 0f);
            SetClip(_negativeDeltaFill, healthUnit / maxUnit, (healthUnit - deltaUnit) / maxUnit);
        }

        if (_healthText != null) _healthText.text = string.Format("{0:0}/{1:0}", healthUnit + shieldUnit, _target.stat.finalMaximumHealth);
    }

    private void SetClip(Image image, float start, float end)
    {
        image.material.SetFloat("_ClipUvLeft", start);
        image.material.SetFloat("_ClipUvRight", 1 - end);
    }



}
