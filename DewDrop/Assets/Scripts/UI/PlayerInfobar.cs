using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfobar : MonoBehaviour, IInfobar
{
    private Entity target;

    private Renderer targetRenderer
    {
        get
        {
            if (_cachedTargetRenderer == null) _cachedTargetRenderer = target.transform.Find("Model").GetComponentInChildren<SkinnedMeshRenderer>();
            return _cachedTargetRenderer;
        }
    }
    private Renderer _cachedTargetRenderer;

    [SerializeField, FoldoutGroup("Required References")]
    private Text _nameText;
    [SerializeField, FoldoutGroup("Required References")]
    private HealthFill _healthFill;
    [SerializeField, FoldoutGroup("Required References")]
    private ManaFill _manaFill;

    [SerializeField]
    private Vector3 _worldOffset;
    [SerializeField]
    private Color _nameColor = Color.white;
    [SerializeField]
    private Color _statusEffectColor = Color.yellow;

    private CanvasGroup _canvasGroup;
    private Canvas _parentCanvas;

    public void SetTarget(Entity target)
    {
        this.target = target;
        _healthFill.Setup(target);
        _manaFill.Setup(target);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        if (target.IsDead() || !targetRenderer.isVisible)
        {
            _canvasGroup.alpha = 0;
            return;
        }
        else
        {
            _canvasGroup.alpha = 1;
        }

        string statusEffectName = StatusEffect.GetImportantStatusEffectName(target);
        if(statusEffectName == "")
        {
            _nameText.text = target.readableName;
            _nameText.color = _nameColor;
        }
        else
        {
            _nameText.text = statusEffectName;
            _nameText.color = _statusEffectColor;
        }

        ((RectTransform)transform).SetWorldPositionForScreenSpaceCamera(target.transform.position + targetRenderer.bounds.size.y * Vector3.up + _worldOffset, _parentCanvas);
    }

}
