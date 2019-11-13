using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class EquippableSocket : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public EquipmentType equipmentType;


    private Image sprite;
    private Image socket;

    public Color[] socketColorByTier = new Color[4];
    public Color emptySocketColor;



    public void OnPointerClick(PointerEventData data)
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target == null || !target.photonView.IsMine) return;
        PlayerItemBelt belt = target.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (belt.equipped[(int)equipmentType] == null) return;

        if(data.button == PointerEventData.InputButton.Left && (GameManager.cachedCurrentNodeType == IngameNodeType.Inventory || GameManager.cachedCurrentNodeType == IngameNodeType.Shop))
        {
            belt.UnequipEquipment((int)equipmentType);
        } else if (data.button == PointerEventData.InputButton.Right && (GameManager.cachedCurrentNodeType == IngameNodeType.Inventory || GameManager.cachedCurrentNodeType == IngameNodeType.Shop))
        {
            belt.UnequipEquipment((int)equipmentType, true);
            belt.inventory[belt.inventory.Count - 1].Disown();
            belt.inventory.RemoveAt(belt.inventory.Count - 1);
        }
        else if (data.button == PointerEventData.InputButton.Middle && GameManager.cachedCurrentNodeType == IngameNodeType.Shop)
        {
            Item item = belt.equipped[(int)equipmentType];
            belt.UnequipEquipment((int)equipmentType, true);
            belt.inventory[belt.inventory.Count - 1].Disown();
            belt.inventory.RemoveAt(belt.inventory.Count - 1);
            ShopManager.instance.SellItem(item);
        }

        if (belt.equipped[(int)equipmentType] == null) DescriptionBox.HideDescription();
    }

    private PointerEventData hover = null;
    public void OnPointerEnter(PointerEventData data)
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target == null || !target.photonView.IsMine) return;
        PlayerItemBelt belt = target.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (belt.equipped[(int)equipmentType] == null) return;
        DescriptionBox.ShowDescription(belt.equipped[(int)equipmentType]);
        hover = data;
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

    private void Awake()
    {
        sprite = transform.Find("Icon/Sprite").GetComponent<Image>();
        socket = transform.Find("Socket").GetComponent<Image>();
    }

    private void Update()
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target == null || !target.photonView.IsMine)
        {
            ResetSocket();
            return;
        }
        PlayerItemBelt belt = target.GetComponent<PlayerItemBelt>();
        if (belt == null)
        {
            ResetSocket();
            return;
        }
        if (belt.equipped[(int)equipmentType] == null)
        {
            ResetSocket();
            return;
        }

        sprite.enabled = true;
        sprite.sprite = belt.equipped[(int)equipmentType].itemIcon ?? null;
        socket.color = socketColorByTier[(int)belt.equipped[(int)equipmentType].itemTier];
    }


    private void ResetSocket()
    {
        sprite.enabled = false;
        socket.color = emptySocketColor;
    }

}
