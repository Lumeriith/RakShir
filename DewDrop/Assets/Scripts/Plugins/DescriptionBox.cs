using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DescriptionBox : MonoBehaviour
{
    public static DescriptionBox instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<DescriptionBox>();
            return _instance;
        }
    }
    private static DescriptionBox _instance;

    private Image image_icon;
    private Text text_title;
    private Text text_subtitle;
    private Text text_description;
    private Text text_secondSubtitle;
    private CanvasGroup group;
    private RectTransform rectTransform;

    private float preferredAlpha = 0f;
    private float alphaSpeed = 7f;

    private AbilityTrigger trigger = null;
    private Consumable consumable = null;
    private Equipment equipment = null;

    private void Awake()
    {
        image_icon = transform.Find("Title Panel/Icon").GetComponent<Image>();
        text_title = transform.Find("Title Panel/Title").GetComponent<Text>();
        text_subtitle = transform.Find("Title Panel/Subtitle").GetComponent<Text>();
        text_secondSubtitle = transform.Find("Title Panel/Second Subtitle").GetComponent<Text>();
        text_description = transform.Find("Description").GetComponent<Text>();
        group = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        group.alpha = 0f;
        preferredAlpha = 0f;
    }

    private void LateUpdate()
    {
        group.alpha = Mathf.MoveTowards(group.alpha, preferredAlpha, alphaSpeed * Time.deltaTime);
        if (group.alpha == 0) return;

        if (consumable != null)
        {
            image_icon.sprite = consumable.itemIcon;
            text_title.text = consumable.itemName;
            switch (consumable.itemTier)
            {
                case ItemTier.Common:
                    text_subtitle.text = "일반 ";
                    break;
                case ItemTier.Rare:
                    text_subtitle.text = "희귀 ";
                    break;
                case ItemTier.Epic:
                    text_subtitle.text = "영웅 ";
                    break;
                case ItemTier.Legendary:
                    text_subtitle.text = "전설 ";
                    break;
            }
            text_subtitle.text += "소모품";
            text_description.text = DescriptionSyntax.Decode(consumable.itemDescription);
            text_secondSubtitle.text = ((int)consumable.value).ToString() + "G";
        }
        else if (trigger != null)
        {
            image_icon.sprite = trigger.abilityIcon;
            text_title.text = trigger.abilityName;
            text_subtitle.text = "";
            if (trigger.manaCost != 0)
            {
                text_subtitle.text += "마나 " + ((int)trigger.manaCost);
            }
            if (trigger.cooldownTime != 0)
            {
                if (text_subtitle.text != "") text_subtitle.text += ", ";
                text_subtitle.text += "재사용 대기시간 " + trigger.cooldownTime.ToString() + "초";
            }
            text_description.text = DescriptionSyntax.Decode(trigger.abilityDescription);
            text_secondSubtitle.text = "";
        }
        else if (equipment != null)
        {
            image_icon.sprite = equipment.itemIcon;
            text_title.text = equipment.itemName;
            switch (equipment.itemTier)
            {
                case ItemTier.Common:
                    text_subtitle.text = "일반 ";
                    break;
                case ItemTier.Rare:
                    text_subtitle.text = "희귀 ";
                    break;
                case ItemTier.Epic:
                    text_subtitle.text = "영웅 ";
                    break;
                case ItemTier.Legendary:
                    text_subtitle.text = "전설 ";
                    break;
            }
            switch (equipment.type)
            {
                case EquipmentType.Helmet:
                    text_subtitle.text += "투구, 지속 효과";
                    break;
                case EquipmentType.Armor:
                    text_subtitle.text += "갑옷, W";
                    break;
                case EquipmentType.Boots:
                    text_subtitle.text += "신발, E";
                    break;
                case EquipmentType.Weapon:
                    text_subtitle.text += "무기, 기본 공격, Q, R";
                    break;
                case EquipmentType.Ring:
                    text_subtitle.text += "반지, Space";
                    break;
            }
            text_description.text = DescriptionSyntax.Decode(equipment.itemDescription);
            text_secondSubtitle.text = ((int)equipment.value).ToString() + "G";
        }

        if (preferredAlpha == 0) return;
        Vector2 position = Input.mousePosition;
        Vector2 size = rectTransform.sizeDelta;
        float margin = 10f;
        position.x -= size.x / 2f;
        position.y += 10f;
        if (position.x < margin) position.x = margin;
        else if (position.x + size.x > Screen.width - margin) position.x = Screen.width - margin - size.x;
        if (position.y < margin) position.y = margin;
        else if (position.y + size.y > Screen.height - margin) position.y = Screen.height - margin - size.y;
        rectTransform.anchoredPosition = position;
    }
    public static void ShowDescription(AbilityTrigger trigger)
    {
        instance.consumable = null;
        instance.equipment = null;
        instance.trigger = trigger;
        instance.preferredAlpha = 1f;
    }

    public static void ShowDescription(Consumable consumable)
    {
        instance.consumable = consumable;
        instance.equipment = null;
        instance.trigger = null;
        instance.preferredAlpha = 1f;
    }

    public static void ShowDescription(Equipment equipment)
    {
        instance.equipment = equipment;
        instance.trigger = null;
        instance.consumable = null;
        instance.preferredAlpha = 1f;
    }

    public static void HideDescription()
    {
        instance.preferredAlpha = 0f;
    }



}