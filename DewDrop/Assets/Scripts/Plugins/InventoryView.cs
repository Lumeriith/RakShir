using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    public static InventoryView instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<InventoryView>();
            return _instance;
        }
    }
    private static InventoryView _instance;

    public List<Color> socketStatusOverlayColors;

    private Image floatingItem;

    private PlayerInventory inventory
    {
        get
        {
            if (_inventory == null) _inventory = UnitControlManager.instance.selectedUnit.GetComponent<PlayerInventory>();
            return _inventory;
        }
    }

    private PlayerInventory _inventory;

    private void Awake()
    {
        floatingItem = transform.Find<Image>("Floating Item");
        floatingItem.enabled = false;
    }

    public void SocketDragStarted(ItemSocket socket)
    {
        floatingItem.enabled = true;
        floatingItem.sprite = socket.item.itemIcon;
    }

    public void SocketDragging(ItemSocket socket)
    {
        floatingItem.transform.position = Input.mousePosition;
    }

    public void SocketStartHoveringOverSocket(ItemSocket socket, ItemSocket to)
    {
        if(to.type == ItemSocket.SocketType.Belt)
        {
            if (socket.item as Consumable != null) to.status = ItemSocket.HighlightStatus.ValidDrop;
            else to.status = ItemSocket.HighlightStatus.InvalidDrop;
        }
        else if (to.type == ItemSocket.SocketType.Equipped)
        {
            Equipment equipment = socket.item as Equipment;
            Gem gem = socket.item as Gem;
            if (equipment == null && gem == null) to.status = ItemSocket.HighlightStatus.InvalidDrop;
            else if (gem != null && to.index != 0) to.status = ItemSocket.HighlightStatus.ValidDrop;
            else if (equipment != null && (int)equipment.type == to.index) to.status = ItemSocket.HighlightStatus.ValidDrop;
            else to.status = ItemSocket.HighlightStatus.InvalidDrop;
        }
        else
        {
            if(socket.type == ItemSocket.SocketType.Inventory || to.item == null)
            {
                to.status = ItemSocket.HighlightStatus.ValidDrop;
            }
            else if(socket.type == ItemSocket.SocketType.Equipped && to.item as Equipment != null && (int)(to.item as Equipment).type == socket.index)
            {
                to.status = ItemSocket.HighlightStatus.ValidDrop;
            }
            else if (socket.type == ItemSocket.SocketType.Belt && to.item as Consumable != null)
            {
                to.status = ItemSocket.HighlightStatus.ValidDrop;
            }
            else
            {
                to.status = ItemSocket.HighlightStatus.InvalidDrop;
            }
        }
    }

    public void SocketDroppedOnSocket(ItemSocket socket, ItemSocket to)
    {
        floatingItem.enabled = false;
        to.status = ItemSocket.HighlightStatus.MouseOver;
        
        if(socket.type == ItemSocket.SocketType.Belt && to.type == ItemSocket.SocketType.Belt)
        {
            inventory.SwapBeltAndBelt(socket.index, to.index);
        } else if (socket.type == ItemSocket.SocketType.Inventory && to.type == ItemSocket.SocketType.Belt)
        {
            inventory.SwapInventoryAndBelt(socket.index, to.index);
        } else if (socket.type == ItemSocket.SocketType.Belt && to.type == ItemSocket.SocketType.Inventory)
        {
            inventory.SwapInventoryAndBelt(to.index, socket.index);
        } else if (socket.type == ItemSocket.SocketType.Inventory && to.type == ItemSocket.SocketType.Equipped)
        {
            inventory.SwapInventoryAndEquipped(socket.index, to.index);
        } else if (socket.type == ItemSocket.SocketType.Equipped && to.type == ItemSocket.SocketType.Inventory)
        {
            inventory.SwapInventoryAndEquipped(to.index, socket.index);
        } else if (socket.type == ItemSocket.SocketType.Inventory && to.type == ItemSocket.SocketType.Inventory)
        {
            inventory.SwapInventoryAndInventory(socket.index, to.index);
        }
    }

    public void SocketDroppedOutside(ItemSocket socket)
    {
        floatingItem.enabled = false;
        if (socket.type == ItemSocket.SocketType.Belt) inventory.DropItemFromBelt(socket.index);
        else if (socket.type == ItemSocket.SocketType.Equipped) inventory.DropItemFromEquipped(socket.index);
        else if (socket.type == ItemSocket.SocketType.Inventory) inventory.DropItemFromInventory(socket.index);
    }

    public void SocketDroppedOnNothing(ItemSocket socket)
    {
        floatingItem.enabled = false;
    }

    private void OnDisable()
    {
        floatingItem.enabled = false;
    }

    public void SocketDefaultActionCalled(ItemSocket socket) // Double-clicked on a socket or pressed E on it
    {
        if (socket.type == ItemSocket.SocketType.Belt) inventory.MoveConsumableFromBeltToInventory(socket.index);
        else if (socket.type == ItemSocket.SocketType.Equipped) inventory.UnequipEquipment(socket.index);
        else
        {
            Equipment equipment = socket.item as Equipment;
            Consumable consumable = socket.item as Consumable;
            if (equipment != null)
            {
                inventory.EquipEquipmentFromInventory(socket.index);
            }
            else if (consumable != null)
            {
                inventory.MoveConsumableFromInventoryToBelt(socket.index);
            }
        }
    }

    public void SocketDropActionCalled(ItemSocket socket)
    {
        if (socket.type == ItemSocket.SocketType.Belt) inventory.DropItemFromBelt(socket.index);
        else if (socket.type == ItemSocket.SocketType.Equipped) inventory.DropItemFromEquipped(socket.index);
        else if (socket.type == ItemSocket.SocketType.Inventory) inventory.DropItemFromInventory(socket.index);
    }


}
