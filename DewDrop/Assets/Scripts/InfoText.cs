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

    private void Awake()
    {
        image_image = GetComponent<Image>();
        text_text = transform.Find("Text").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        main = Camera.main;
        image_icon = transform.Find("Icon").GetComponent<Image>();
    }

    private void Start()
    {
        if (follow.GetComponent<Consumable>() != null)
        {
            image_icon.sprite = consumableIcon;
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