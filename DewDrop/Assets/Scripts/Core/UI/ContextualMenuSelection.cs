using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ContextualMenuSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Image image;
    private ContextualMenu parentMenu;

    public int index = 0;
    public Color defaultColor;
    public Color mouseOverColor;
    public Color mouseDownColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        parentMenu = transform.parent.GetComponent<ContextualMenu>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = mouseOverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = defaultColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.color = mouseDownColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition, GUIManager.instance.uiCamera)) image.color = mouseOverColor;
        else image.color = defaultColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        parentMenu.SelectionClicked(index);
    }
}
