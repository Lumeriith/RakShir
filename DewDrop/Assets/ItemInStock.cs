using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemInStock : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image tierGradient;
    private Image icon;
    private new Text name;
    private Text subtitle;
    private Text value;

    private CanvasGroup canvasGroup;
    private GameObject gobj_outOfStock;

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

    public void OnPointerClick(PointerEventData data)
    {
        if (GameManager.instance.localPlayer == null) return;
        if (ShopManager.instance.currentStock.Count <= index) return;


        if (data.button == PointerEventData.InputButton.Middle && GameManager.cachedCurrentNodeType == IngameNodeType.Shop)
        {
            ShopManager.instance.TryBuyItem(index);
        }
    }

    private PointerEventData hover;
    public void OnPointerEnter(PointerEventData data)
    {
        if (ShopManager.instance.currentStock.Count <= index) return;
        if (ShopManager.instance.currentStock[index] as Equipment != null)
        {
            DescriptionBox.ShowDescription(ShopManager.instance.currentStock[index] as Equipment);
            hover = data;
        }
        else if (ShopManager.instance.currentStock[index] as Consumable != null)
        {
            DescriptionBox.ShowDescription(ShopManager.instance.currentStock[index] as Consumable);
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
        if (hover != null)
        {
            DescriptionBox.HideDescription();
            hover = null;
        }
    }


    private void ShowItem()
    {
        canvasGroup.alpha = 1f;
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
        value = transform.Find("Value").GetComponent<Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        gobj_outOfStock = transform.Find("OutOfStock").gameObject;
    }

    void Update()
    {
        if (ShopManager.instance.currentStock.Count <= index || ShopManager.instance.currentStock[index] == null)
        {
            HideItem();
            return;
        }
        ShowItem();
        icon.sprite = ShopManager.instance.currentStock[index].itemIcon;
        name.text = ShopManager.instance.currentStock[index].itemName;
        gobj_outOfStock.SetActive(ShopManager.instance.currentStockNumber[index] <= 0);

        string subtitle = "";
        Color color = Color.white;

        value.text = "<color=grey>" + ShopManager.instance.currentStockNumber[index] + "개 남음</color> 구매가격 " + (int)ShopManager.instance.currentStock[index].value + "G";

        if (ShopManager.instance.currentStock[index].itemTier == ItemTier.Common)
        {
            color = commonColor;
            subtitle = "<b>일반 </b>";
        }
        else if (ShopManager.instance.currentStock[index].itemTier == ItemTier.Rare)
        {
            color = rareColor;
            subtitle = "<b>희귀 </b>";
        }
        else if (ShopManager.instance.currentStock[index].itemTier == ItemTier.Epic)
        {
            color = epicColor;
            subtitle = "<b>영웅 </b>";
        }
        else if (ShopManager.instance.currentStock[index].itemTier == ItemTier.Legendary)
        {
            color = legendaryColor;
            subtitle = "<b>전설 </b>";
        }

        name.color = color;
        color.a = color.a / 16f;
        tierGradient.color = color;

        Consumable consumable = ShopManager.instance.currentStock[index] as Consumable;
        if (consumable != null)
        {
            this.subtitle.color = consumableSubtitleColor;
            subtitle += "<b>소모품</b> ";
        }

        Equipment equipment = ShopManager.instance.currentStock[index] as Equipment;
        if (equipment != null)
        {
            if (equipment.type == EquipmentType.Weapon)
            {
                subtitle += "<b>무기</b> <i>공격, Q, R</i>";
                this.subtitle.color = weaponSubtitleColor;
            }
            else if (equipment.type == EquipmentType.Armor)
            {
                subtitle += "<b>갑옷</b> <i>W</i>";
                this.subtitle.color = armorSubtitleColor;
            }
            else if (equipment.type == EquipmentType.Boots)
            {
                subtitle += "<b>신발</b> <i>E</i>";
                this.subtitle.color = bootsSubtitleColor;
            }
            else if (equipment.type == EquipmentType.Helmet)
            {
                subtitle += "<b>투구</b> <i>지속 효과</i>";
                this.subtitle.color = helmetSubtitleColor;
            }
            else if (equipment.type == EquipmentType.Ring)
            {
                subtitle += "<b>반지</b> <i>Space</i>";
                this.subtitle.color = ringSubtitleColor;
            }
        }

        this.subtitle.text = subtitle;
    }
}
