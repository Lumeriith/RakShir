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

    public ContextualMenu contextualMenuPrefab;

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
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, Input.mousePosition, GUIManager.instance.uiCamera, out Vector2 localPoint);
        floatingItem.transform.localPosition = localPoint;
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
            else if (gem != null && to.index != 0 && to.item != null)
            {
                Equipment toEquip = to.item as Equipment;
                bool found = false;
                for (int i = 1; i < toEquip.skillSetReplacements.Length; i++)
                {
                    if(toEquip.skillSetReplacements[i] != null && toEquip.skillSetReplacements[i].connectedGems.Count < AbilityTrigger.maxGemPerTrigger)
                    {
                        found = true;
                        break;
                    }
                }
                if(found) to.status = ItemSocket.HighlightStatus.ValidDrop;
                else to.status = ItemSocket.HighlightStatus.InvalidDrop;
            }
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
            if(socket.item as Gem != null && to.item != null)
            {
                // Attempt to attach gem
                Equipment toEquip = to.item as Equipment;
                List<string> selections = new List<string>();
                List<System.Action> callbacks = new List<System.Action>();
                string[] candidates = new string[] { "무기(기본 공격)에 장착", "무기(Q)에 장착", "갑옷(W)에 장착", "장화(E)에 장착", "무기(R)에 장착", "반지(D)에 장착", "헬멧(P)에 장착" };
                for(int i = 1; i < 6; i++)
                {
                    if (toEquip.skillSetReplacements[i] != null && toEquip.skillSetReplacements[i].connectedGems.Count < AbilityTrigger.maxGemPerTrigger)
                    {
                        AbilityTrigger trigger = toEquip.skillSetReplacements[i];
                        selections.Add(candidates[i]);
                        callbacks.Add(() => { inventory.EquipGemFromInventory(socket.index, trigger); });
                    }
                }
                if(callbacks.Count == 1)
                {
                    callbacks[0]();
                } else if (callbacks.Count > 1)
                {
                    ContextualMenu menu = Instantiate(contextualMenuPrefab, to.transform.position, Quaternion.identity, transform);
                    menu.SetSelections(selections.ToArray());
                    menu.SetCallbacks(callbacks.ToArray());
                }
            }
            else
            {
                // Attempt to equip the item
                inventory.SwapInventoryAndEquipped(socket.index, to.index);
            }

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

    public void SocketContextMenuCalled(ItemSocket socket)
    {
        ContextualMenu menu = Instantiate(contextualMenuPrefab, socket.transform.position, socket.transform.rotation, transform);
        if(socket.type == ItemSocket.SocketType.Equipped)
        {
            Equipment equipment = socket.item as Equipment;
            List<string> selections = new List<string>();
            List<System.Action> callbacks = new List<System.Action>();
            
            selections.Add("착용 해제");
            callbacks.Add(() => { SocketDefaultActionCalled(socket); });

            for(int i = 0; i < equipment.skillSetReplacements.Length; i++)
            {
                if(equipment.skillSetReplacements[i] != null && equipment.skillSetReplacements[i].connectedGems.Count > 0)
                {
                    AbilityTrigger trigger = equipment.skillSetReplacements[i];
                    selections.Add("보석 제거 (" + "?QWERDP"[i] + ")");
                    callbacks.Add(() => { inventory.UnequipAllGemsInTrigger(trigger); });
                }
            }

            selections.Add("버리기");
            callbacks.Add(() => { SocketDropActionCalled(socket); });

            menu.SetSelections(selections.ToArray());
            menu.SetCallbacks(callbacks.ToArray());
        }
        else if (socket.type == ItemSocket.SocketType.Belt)
        {
            menu.SetSelections("착용 해제", "버리기");
            menu.SetCallbacks(
                () => { SocketDefaultActionCalled(socket); },
                () => { SocketDropActionCalled(socket); }
                );
        }
        else if (socket.type == ItemSocket.SocketType.Inventory)
        {
            if (socket.item as Equipment != null)
            {
                Equipment equipment = socket.item as Equipment;
                List<string> selections = new List<string>();
                List<System.Action> callbacks = new List<System.Action>();

                selections.Add("착용");
                callbacks.Add(() => { SocketDefaultActionCalled(socket); });

                for (int i = 0; i < equipment.skillSetReplacements.Length; i++)
                {
                    if (equipment.skillSetReplacements[i] != null && equipment.skillSetReplacements[i].connectedGems.Count > 0)
                    {
                        AbilityTrigger trigger = equipment.skillSetReplacements[i];
                        selections.Add("보석 제거 (" + "?QWERDP"[i] + ")");
                        callbacks.Add(() => { inventory.UnequipAllGemsInTrigger(trigger); });
                    }
                }

                selections.Add("버리기");
                callbacks.Add(() => { SocketDropActionCalled(socket); });

                menu.SetSelections(selections.ToArray());
                menu.SetCallbacks(callbacks.ToArray());
            }
            else if (socket.item as Consumable != null)
            {
                menu.SetSelections("착용", "버리기");
                menu.SetCallbacks(
                    () => { SocketDefaultActionCalled(socket); },
                    () => { SocketDropActionCalled(socket); }
                    );
            }
            else if (socket.item as Gem != null)
            {
                List<string> selections = new List<string>();
                List<System.Action> callbacks = new List<System.Action>();
                string[] candidates = { "무기(기본 공격)에 장착", "무기(Q)에 장착", "갑옷(W)에 장착", "장화(E)에 장착", "무기(R)에 장착", "반지(D)에 장착", "헬멧(P)에 장착" };
                for(int i = 1; i < 6; i++)
                {
                    if(inventory.livingThing.control.skillSet[i] != null && inventory.livingThing.control.skillSet[i].connectedGems.Count < AbilityTrigger.maxGemPerTrigger)
                    {
                        AbilityTrigger trigger = inventory.livingThing.control.skillSet[i];
                        selections.Add(candidates[i]);
                        callbacks.Add(() => { inventory.EquipGemFromInventory(socket.index, trigger); });
                    }
                }
                selections.Add("버리기");
                callbacks.Add(() => { SocketDropActionCalled(socket); });
                menu.SetSelections(selections.ToArray());
                menu.SetCallbacks(callbacks.ToArray());
            }
        }

    }

}
