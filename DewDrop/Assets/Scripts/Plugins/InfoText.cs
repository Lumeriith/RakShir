using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InfoText : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool clickable;

    private Image image_image;
    private Image image_icon;
    private Text text_text;
    private CanvasGroup canvasGroup;
    private Camera main;

    public string text;

    public Transform follow;
    public Vector3 offsetUI;

    public System.Action OnClick = () => { };

    public Sprite weaponIcon;
    public Sprite armorIcon;
    public Sprite helmetIcon;
    public Sprite bootsIcon;
    public Sprite ringIcon;
    public Sprite consumableIcon;
    public Sprite moneyIcon;
    public Sprite bookIcon;

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