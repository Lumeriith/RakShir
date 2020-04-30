using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaFill : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private Image _normalFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _positiveDeltaFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Image _negativeDeltaFill;
    [SerializeField, FoldoutGroup("Required References")]
    private Text _manaText;

    private Entity _target;

    private float _previousManaUnit;
    private float _previousManaUnitVelocity;

    [SerializeField]
    private float _deltaSmoothTime = 0.2f;
    [SerializeField]
    private float _deltaNormalizedLerpSpeed = 0.05f;

    private void Awake()
    {
        _normalFill.material = new Material(_normalFill.material);
        _positiveDeltaFill.material = new Material(_positiveDeltaFill.material);
        _negativeDeltaFill.material = new Material(_negativeDeltaFill.material);
    }

    public void Setup(Entity target)
    {
        _target = target;
        if (target != null) UpdateManaFills();
    }

    private void Update()
    {
        if (_target != null) UpdateManaFills();
    }

    private void UpdateManaFills()
    {
        float maxUnit = _target.stat.finalMaximumMana;
        float manaUnit = _target.stat.currentMana;

        _previousManaUnit = Mathf.SmoothDamp(_previousManaUnit, manaUnit, ref _previousManaUnitVelocity, _deltaSmoothTime);
        _previousManaUnit = Mathf.MoveTowards(_previousManaUnit, manaUnit, _deltaNormalizedLerpSpeed * maxUnit * Time.deltaTime);

        float deltaUnit = manaUnit - _previousManaUnit;

        SetClip(_normalFill, 0f, manaUnit / maxUnit);

        if (deltaUnit > 0)
        {
            SetClip(_positiveDeltaFill, (manaUnit - deltaUnit) / maxUnit, manaUnit / maxUnit);
            SetClip(_negativeDeltaFill, 0f, 0f);
        }
        else
        {
            SetClip(_positiveDeltaFill, 0f, 0f);
            SetClip(_negativeDeltaFill, manaUnit / maxUnit, (manaUnit - deltaUnit) / maxUnit);
        }

        if (_manaText != null) _manaText.text = string.Format("{0:0}/{1:0}", manaUnit, _target.stat.finalMaximumHealth);
    }

    private void SetClip(Image image, float start, float end)
    {
        image.material.SetFloat("_ClipUvLeft", start);
        image.material.SetFloat("_ClipUvRight", 1 - end);
    }



}
