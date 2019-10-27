using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Infotext : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool clickable;
    
    private Image image;
    private Text text;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = transform.Find("Text").GetComponent<Text>();
    }

    private void Update()
    {
        
    }


    public void OnPointerEnter(PointerEventData data)
    {
        image.color = new Color(70f, 70f, 70f, 189f);
    }

    public void OnPointerDown(PointerEventData data)
    {
        image.color = new Color(140f, 140f, 140f, 189f);
    }

    public void OnPointerUp(PointerEventData data)
    {
        image.color = new Color(70f, 70f, 70f, 189f);
    }

    public void OnPointerExit(PointerEventData data)
    {
        image.color = new Color(0f, 0f, 0f, 189f);
    }

    public void OnPointerClick(PointerEventData data)
    {

    }

}
