using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterInfobar : MonoBehaviour, IInfobar
{


    private Renderer _targetRenderer
    {
        get
        {
            if (_cachedTargetRenderer == null) _cachedTargetRenderer = _target.transform.Find("Model").GetComponentInChildren<Renderer>();
            return _cachedTargetRenderer;
        }
    }
    private Renderer _cachedTargetRenderer;

    
    [SerializeField, FoldoutGroup("Required References")]
    private HealthFill _healthFill;

    [SerializeField]
    private Vector3 _worldOffset;

    private CanvasGroup _canvasGroup;
    private Entity _target;
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    

    public void SetTarget(Entity target)
    {
        this._target = target;
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = (RectTransform)transform;
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (_target.IsDead() || !_targetRenderer.isVisible)
        {
            _canvasGroup.alpha = 0f;
            _healthFill.enabled = false;
            return;
        }
        else
        {
            _healthFill.enabled = true;
            _canvasGroup.alpha = 1f;
        }
        
        _healthFill.Setup(_target);

        _rectTransform.SetWorldPositionForScreenSpaceCamera(_target.transform.position + _targetRenderer.bounds.size.y * Vector3.up + _worldOffset, _parentCanvas);
    }
}
