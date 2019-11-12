using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ConsumableSocket : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public int consumableIndex;


    private Image icon;
    private Image socket;

    public Color socketColor;
    public Color emptySocketColor;

    public void OnPointerClick(PointerEventData data)
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target == null || !target.photonView.IsMine) return;
        PlayerItemBelt belt = target.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (belt.consumableBelt[consumableIndex] == null) return;
        belt.MoveConsumableFromBeltToInventory(consumableIndex);
    }

    private PointerEventData hover = null;
    public void OnPointerEnter(PointerEventData data)
    {
        LivingThing target = GameManager.instance.localPlayer;
        if (target == null || !target.photonView.IsMine) return;
        PlayerItemBelt belt = target.GetComponent<PlayerItemBelt>();
        if (belt == null) return;
        if (belt.consumableBelt[consumableIndex] == null) return;
        DescriptionBox.ShowDescription(belt.consumableBelt[consumableIndex]);
        hover = data;
    }

    public void OnPointerExit(PointerEventData data)
    {
        DescriptionBox.HideDescription();
        hover = null;
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
        icon = transform.Find("Icon").GetComponent<Image>();
        socket = transform.Find("Socket").GetComponent<Image>();
    }

    private void Update()
    {
        LivingThing target = UnitControlManager.instance.selectedUnit;
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
        if (belt.consumableBelt[consumableIndex] == null)
        {
            ResetSocket();
            return;
        }

        icon.enabled = true;
        icon.sprite = belt.consumableBelt[consumableIndex].itemIcon ?? null;
        socket.color = socketColor;
    }


    private void ResetSocket()
    {
        icon.enabled = false;
        socket.color = emptySocketColor;
    }
}
