using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class EquippableSocket : MonoBehaviour, IPointerClickHandler
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
        belt.UnequipEquipment((int)equipmentType);
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
