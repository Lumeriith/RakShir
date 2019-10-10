using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippableSocket : MonoBehaviour
{

    public EquipmentType equipmentType;


    private Image sprite;
    private Image socket;

    public Color[] socketColorByTier = new Color[4];
    public Color emptySocketColor;


    private void Awake()
    {
        sprite = transform.Find("Icon/Sprite").GetComponent<Image>();
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
        if (belt.equipped[(int)equipmentType] == null)
        {
            ResetSocket();
            return;
        }

        sprite.enabled = true;
        sprite.sprite = belt.equipped[(int)equipmentType].equippableIcon ?? null;
        socket.color = socketColorByTier[(int)belt.equipped[(int)equipmentType].tier];
    }


    private void ResetSocket()
    {
        sprite.enabled = false;
        socket.color = emptySocketColor;
    }

}
