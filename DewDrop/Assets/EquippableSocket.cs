using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippableSocket : MonoBehaviour
{

    public int equippableIndex;


    private Image icon;

    private void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
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
        if (belt.equipped[equippableIndex] == null)
        {
            ResetSocket();
            return;
        }
        icon.enabled = true;
        icon.sprite = belt.equipped[equippableIndex].equippableIcon ?? null; 
        
    }


    private void ResetSocket()
    {
        icon.enabled = false;
    }

}
