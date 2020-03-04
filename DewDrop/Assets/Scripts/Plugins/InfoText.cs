using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InfoText : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public static SortedList<float, Rect> drawnInfoTextRects = new SortedList<float, Rect>(new DuplicateKeyComparer<float>());

    public bool clickable;

    private Image image_image;
    private Image image_icon;
    private Text text_text;
    private CanvasGroup canvasGroup;
    private Camera main;
    private RectTransform rectTransform;

    public string text;

    public Transform follow;
    public Vector3 offsetUI;
    public Vector3 offsetWorld;

    public System.Action OnClick = () => { };

    public Sprite weaponIcon;
    public Sprite armorIcon;
    public Sprite helmetIcon;
    public Sprite bootsIcon;
    public Sprite ringIcon;
    public Sprite consumableIcon;
    public Sprite moneyIcon;
    public Sprite bookIcon;
    public Sprite gemIcon;

    private float creationTime;
    private void Awake()
    {
        image_image = GetComponent<Image>();
        text_text = transform.Find("Text").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        main = Camera.main;
        image_icon = transform.Find("Icon").GetComponent<Image>();
        creationTime = Time.time;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Consumable cons = follow.GetComponent<Consumable>();
        if (cons != null)
        {
            image_icon.sprite = consumableIcon;
            if (cons as cons_GoldPouch != null) image_icon.sprite = moneyIcon;
            if (cons as cons_BookOfAgility != null || cons as cons_BookOfStrength != null || cons as cons_BookOfIntelligence != null) image_icon.sprite = bookIcon;
            return;
        }

        Equipment equip = follow.GetComponent<Equipment>();
        if (equip != null)
        {
            switch (equip.type)
            {
                case EquipmentType.Helmet:
                    image_icon.sprite = helmetIcon;
                    break;
                case EquipmentType.Armor:
                    image_icon.sprite = armorIcon;
                    break;
                case EquipmentType.Boots:
                    image_icon.sprite = bootsIcon;
                    break;
                case EquipmentType.Weapon:
                    image_icon.sprite = weaponIcon;
                    break;
                case EquipmentType.Ring:
                    image_icon.sprite = ringIcon;
                    break;
            }
        }
        if(follow.GetComponent<Gem>() != null)
        {
            image_icon.sprite = gemIcon;
        }
    }

    private void Update()
    {
        drawnInfoTextRects.Clear();
    }

    private void LateUpdate() {
        if (follow == null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            if(Time.time - creationTime > 1f) Destroy(gameObject);
            return;
        }
        Vector3 viewportPoint = main.WorldToViewportPoint(follow.transform.position);
        if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1||viewportPoint.z < 0|| !follow.gameObject.activeInHierarchy)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else
        {

            text_text.text = text;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            transform.position = main.WorldToScreenPoint(follow.transform.position + offsetWorld) + offsetUI;
            Rect myRect = new Rect((Vector2)transform.position - rectTransform.sizeDelta/2f, rectTransform.sizeDelta);
            for (int i = 0; i < drawnInfoTextRects.Count; i++)
            {
                if (myRect.Overlaps(drawnInfoTextRects.Values[i]))
                {
                    transform.position += Vector3.up * (drawnInfoTextRects.Values[i].yMax - myRect.yMin);
                    myRect = new Rect((Vector2)transform.position - rectTransform.sizeDelta / 2f, rectTransform.sizeDelta);

                }
            }
            drawnInfoTextRects.Add(myRect.y, myRect);
        }
    }



    public void OnPointerDown(PointerEventData data)
    {
        if(data.button == PointerEventData.InputButton.Left) image_image.color = new Color(180f / 255f, 180f / 255f, 180f / 255f, 200f / 255f);

    }

    public void OnPointerUp(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left) image_image.color = new Color(0f, 0f, 0f, 189f / 255f);
    }


    public void OnPointerClick(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left) OnClick.Invoke();
    }


}

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater
        else
            return result;
    }

    #endregion
}