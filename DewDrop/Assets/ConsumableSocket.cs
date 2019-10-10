using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ConsumableSocket : MonoBehaviour
{

    public int consumableIndex;


    private Image icon;
    private Image socket;

    public Color socketColor;
    public Color emptySocketColor;


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
        if (belt.consumables[consumableIndex] == null)
        {
            ResetSocket();
            return;
        }

        icon.enabled = true;
        icon.sprite = belt.consumables[consumableIndex].sprite ?? null;
        socket.color = socketColor;
    }


    private void ResetSocket()
    {
        icon.enabled = false;
        socket.color = emptySocketColor;
    }
}
