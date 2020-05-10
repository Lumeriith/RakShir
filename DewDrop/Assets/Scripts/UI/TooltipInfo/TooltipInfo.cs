using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TooltipInfo : SingletonBehaviour<TooltipInfo>
{
    private const float AlphaLerpSpeed = 10f;
    [SerializeField, FoldoutGroup("Required References")]
    private CanvasGroup _canvasGroup;

    [SerializeField, FoldoutGroup("Required References")]
    private TooltipAbilityGroup _abilityGroup;
    [SerializeField, FoldoutGroup("Required References")]
    private TooltipGenericItemGroup _genericItemGroup;

    private bool _isShown;





    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }


    private void Update()
    {
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _isShown ? 1f : 0f, AlphaLerpSpeed * Time.deltaTime);
        if (_isShown)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, Input.mousePosition, GUIManager.instance.uiCamera, out Vector2 localPoint);
            transform.localPosition = localPoint;
        }
    }

    public void Show(Item item)
    {
        _isShown = true;
        _canvasGroup.alpha = 0f;

        _abilityGroup.gameObject.SetActive(false);
        _genericItemGroup.gameObject.SetActive(true);

        _genericItemGroup.Setup(item);
    }

    public void Show(int index)
    {
        _isShown = true;
        _canvasGroup.alpha = 0f;

        _abilityGroup.gameObject.SetActive(true);
        _genericItemGroup.gameObject.SetActive(false);

        _abilityGroup.Setup(index);
    }

    public void Hide()
    {
        _isShown = false;
    }
}
