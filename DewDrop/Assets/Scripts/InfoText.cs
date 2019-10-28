using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InfoText : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool clickable;

    private Image image_image;
    private Text text_text;
    private CanvasGroup canvasGroup;
    private Camera main;

    public string text;

    public Transform follow;
    public Vector3 offsetUI;

    public System.Action OnClick = () => { };

    private void Awake()
    {
        image_image = GetComponent<Image>();
        text_text = transform.Find("Text").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        main = Camera.main;
    }


    private void LateUpdate()
    {
        text_text.text = text;
        if(follow == null || !follow.gameObject.activeInHierarchy)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            transform.position = main.WorldToScreenPoint(follow.transform.position) + offsetUI;
        }
    }



    public void OnPointerDown(PointerEventData data)
    {
        image_image.color = new Color(180f / 255f, 180f / 255f, 180f / 255f, 200f / 255f);
    }

    public void OnPointerUp(PointerEventData data)
    {
        image_image.color = new Color(0f, 0f, 0f, 189f / 255f);
    }


    public void OnPointerClick(PointerEventData data)
    {
        OnClick.Invoke();
    }

}
