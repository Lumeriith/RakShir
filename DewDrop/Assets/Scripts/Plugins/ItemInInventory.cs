using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemInInventory : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image tierGradient;
    private Image icon;
    private new Text name;
    private Text subtitle;

    private PlayerInventory belt;

    private CanvasGroup canvasGroup;

    private Text value;

    public int index;
    public Color consumableSubtitleColor;
    public Color weaponSubtitleColor;
    public Color armorSubtitleColor;
    public Color bootsSubtitleColor;
    public Color helmetSubtitleColor;
    public Color ringSubtitleColor;

    public Color legendaryColor;
    public Color epicColor;
    public Color rareColor;
    public Color commonColor;

    public bool showValue = false;

    public void OnPointerClick(PointerEventData data)
    {
        if (GameManager.instance.localPlayer == null) return;
        if (belt.inventory.Count <= index || belt.inventory[index] == null) return;



        if (data.button == PointerEventData.InputButton.Left && (GameManager.cachedCurrentNodeType == IngameNodeType.Inventory || GameManager.cachedCurrentNodeType == IngameNodeType.Shop))
        {
            if (belt.inventory[index] as Equipment != null)
            {
                belt.EquipEquipmentFromInventory(index);
            }
            else if (belt.inventory[index] as Consumable != null)
            {
                belt.MoveConsumableFromInventoryToBelt(index);
            }
        }
        else if (data.button == PointerEventData.InputButton.Right && (GameManager.cachedCurrentNodeType == IngameNodeType.Inventory || GameManager.cachedCurrentNodeType == IngameNodeType.Shop))
        {
            belt.inventory[index].Disown();
            belt.inventory.RemoveAt(index);
        }
        else if (data.button == PointerEventData.InputButton.Middle && GameManager.cachedCurrentNodeType == IngameNodeType.Shop)
        {
            Item item = belt.inventory[index];
            belt.inventory[index].Disown();
            belt.inventory.RemoveAt(index);
            ShopManager.instance.SellItem(item);
        }

        if (belt.inventory.Count <= index || belt.inventory[index] == null) DescriptionBox.HideDescription();
        else
        {
            if (belt.inventory[index] as Equipment != null)
            {
                DescriptionBox.ShowDescription(belt.inventory[index] as Equipment);
            }
            else if (belt.inventory[index] as Consumable != null)
            {
                DescriptionBox.ShowDescription(belt.inventory[index] as Consumable);
            }
        }
    }

    private PointerEventData hover;
    public void OnPointerEnter(PointerEventData data)
    {
        if (GameManager.instance.localPlayer == null) return;
        if (belt == null) belt = GameManager.instance.localPlayer.GetComponent<PlayerInventory>();
        if (belt.inventory.Count <= index || belt.inventory[index] == null) return;

        if (belt.inventory[index] as Equipment != null)
        {
            DescriptionBox.ShowDescription(belt.inventory[index] as Equipment);
            hover = data;
        }
        else if (belt.inventory[index] as Consumable != null)
        {
            DescriptionBox.ShowDescription(belt.inventory[index] as Consumable);
            hover = data;
        }

    }
    public void OnPointerExit(PointerEventData data)
    {
        if (hover != null)
        {
            DescriptionBox.HideDescription();
            hover = null;
        }
    }
    private void OnDisable()
    {
        if(hover != null)
        {
            DescriptionBox.HideDescription();
            hover = null;
        }
    }


    private void ShowItem()
    {
        canvasGroup.alpha = 1f  ;
    }

    private void HideItem()
    {
        canvasGroup.alpha = 0f;
    }

    private void Awake()
    {
        tierGradient = transform.Find("Tier Gradient").GetComponent<Image>();
        icon = transform.Find("Icon").GetComponent<Image>();
        name = transform.Find("Name").GetComponent<Text>();
        subtitle = transform.Find("Subtitle").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        value = transform.Find("Value").GetComponent<Text>();
    }

    void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        if (belt == null) belt = GameManager.instance.localPlayer.GetComponent<PlayerInventory>();
        if (belt.inventory.Count <= index || belt.inventory[index] == null)
        {
            HideItem();
            return;
        }
        ShowItem();
        icon.sprite = belt.inventory[index].itemIcon;
        name.text = belt.inventory[index].itemName;
        value.text = "판매가격 " + (int)(belt.inventory[index].value * ShopManager.instance.itemSellingValueModifier) + "G";
        value.enabled = showValue;
        string subtitle = "";
        Color color = Color.white;



        if (belt.inventory[index].itemTier == ItemTier.Common)
        {
            color = commonColor;
            subtitle = "<b>일반 </b>";
        }
        else if (belt.inventory[index].itemTier == ItemTier.Rare)
        {
            color = rareColor;
            subtitle = "<b>희귀 </b>";
        }
        else if (belt.inventory[index].itemTier == ItemTier.Epic)
        {
            color = epicColor;
            subtitle = "<b>영웅 </b>";
        }
        else if (belt.inventory[index].itemTier == ItemTier.Legendary)
        {
            color = legendaryColor;
            subtitle = "<b>전설 </b>";
        }

        name.color = color;
        color.a = color.a / 16f;
        tierGradient.color = color;

        Consumable consumable = belt.inventory[index] as Consumable;
        if (consumable != null)
        {
            this.subtitle.color = consumableSubtitleColor;
            subtitle += "<b>소모품</b> ";
        }

        Equipment equipment = belt.inventory[index] as Equipment;
        if(equipment != null)
        {
            if(equipment.type == EquipmentType.Weapon)
            {
                subtitle += "<b>무기</b> <i>공격, Q, R</i>";
                this.subtitle.color = weaponSubtitleColor;
            }
            else if (equipment.type == EquipmentType.Armor)
            {
                subtitle += "<b>갑옷</b> <i>W</i>";
                this.subtitle.color = armorSubtitleColor;
            } else if (equipment.type == EquipmentType.Boots)
            {
                subtitle += "<b>신발</b> <i>E</i>";
                this.subtitle.color = bootsSubtitleColor;
            } else if (equipment.type== EquipmentType.Helmet)
            {
                subtitle += "<b>투구</b> <i>지속 효과</i>";
                this.subtitle.color = helmetSubtitleColor;
            } else if (equipment.type == EquipmentType.Ring)
            {
                subtitle += "<b>반지</b> <i>Space</i>";
                this.subtitle.color = ringSubtitleColor;
            }
        }

        this.subtitle.text = subtitle;
    }
}
