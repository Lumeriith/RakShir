using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSocket : MonoBehaviour, IDragHandler, IDropHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private static bool isDragSuccessful;
    private static ItemSocket draggedSocket;

    public enum SocketType
    {
        Equipped, Belt, Inventory
    }

    public enum HighlightStatus
    {
        None, MouseOver, ValidDrop, InvalidDrop, Pressed
    }

    public SocketType type;
    public HighlightStatus status
    {
        get { return _status; }
        set { _status = value; StatusChanged(); }
    }
    private HighlightStatus _status;

    public Item item { get; private set; }

    public int index;

    private Image backgroundIcon;
    private GameObject fill;
    private CanvasGroup fillCanvasGroup;
    private Image itemIcon;
    private Image rarity;
    private Image overlay;

    private PlayerInventory inventory;

    private bool firstUpdate = true;

    private Image[] gemSprites;
    private bool shouldUpdateGems = true;

    private void Awake()
    {
        Transform backgroundIconTransform = transform.Find("Background Icon");
        if (backgroundIconTransform != null) backgroundIcon = backgroundIconTransform.GetComponent<Image>();
        fill = transform.Find("Fill").gameObject;
        fillCanvasGroup = fill.GetComponent<CanvasGroup>();
        itemIcon = transform.Find<Image>("Fill/Item Icon");
        rarity = transform.Find<Image>("Fill/Rarity");
        overlay = transform.Find<Image>("Overlay");
        gemSprites = new Image[] {
            transform.Find<Image>("Fill/Gem 0"),
            transform.Find<Image>("Fill/Gem 1"),
            transform.Find<Image>("Fill/Gem 2"),
            transform.Find<Image>("Fill/Gem 3"),
            transform.Find<Image>("Fill/Gem 4"),
            transform.Find<Image>("Fill/Gem 5")
        };
    }

    private void Update()
    {
        if (UnitControlManager.instance.selectedUnit == null) return;
        if (inventory == null || UnitControlManager.instance.selectedUnit != inventory.livingThing)
        {
            inventory = UnitControlManager.instance.selectedUnit.GetComponent<PlayerInventory>();
            if (inventory == null) return;
        }

        Item foundItem = null;

        if(type == SocketType.Belt)
        {
            foundItem = inventory.consumableBelt.Length > index ? inventory.consumableBelt[index] : null;
        }
        else if (type == SocketType.Equipped)
        {
            foundItem = inventory.equipped.Length > index ? inventory.equipped[index] : null;
        }
        else if (type == SocketType.Inventory)
        {
            foundItem = inventory.inventory.Length > index ? inventory.inventory[index] : null;
        }

        if(item != foundItem || firstUpdate)
        {
            firstUpdate = false;
            item = foundItem;
            if(foundItem == null)
            {
                fill.SetActive(false);
            }
            else
            {
                fill.SetActive(true);
                itemIcon.sprite = foundItem.itemIcon;
            }

            if(foundItem as Equipment != null)
            {
                shouldUpdateGems = true;
                for (int i = 0; i < gemSprites.Length; i++)
                {
                    gemSprites[i].enabled = true;
                }
            }
            else
            {
                shouldUpdateGems = false;
                for(int i = 0; i < gemSprites.Length; i++)
                {
                    gemSprites[i].enabled = false;
                }
            }
        }

        if (shouldUpdateGems)
        {
            Equipment equipment = item as Equipment;
            int index;
            for(int i = 0; i < gemSprites.Length; i++)
            {
                gemSprites[i].enabled = false; // TODO: Fix this following horrendously inefficient-looking part of the code
                // Enabling and disabling image every frame can only be so efficient!
            }

            for(int i = 0;i< equipment.skillSetReplacements.Length; i++)
            {
                if (equipment.skillSetReplacements[i] == null || equipment.skillSetReplacements[i].connectedGems.Count == 0) continue;
                for(int j = 0; j < equipment.skillSetReplacements[i].connectedGems.Count; j++)
                {
                    index = i == 4 ? 3 + j : j;
                    if (index >= 0 && index < gemSprites.Length)
                    {
                        gemSprites[index].sprite = equipment.skillSetReplacements[i].connectedGems[j].itemIcon;
                        gemSprites[index].enabled = true;
                    }
                }
            }
        }

        if(item != null && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InventoryView.instance.SocketDefaultActionCalled(this);
            } else if (Input.GetKeyDown(KeyCode.R))
            {
                InventoryView.instance.SocketDropActionCalled(this);
            }
        }
    }




    public void OnPointerDown(PointerEventData eventData)
    {
        status = HighlightStatus.Pressed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition)) status = HighlightStatus.MouseOver;
        else status = HighlightStatus.None;
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        status = HighlightStatus.None;
        if (item == null) return;
        InventoryView.instance.SocketDragStarted(this);
        fillCanvasGroup.alpha = .2f;
        isDragSuccessful = false;
        draggedSocket = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null) return;
        InventoryView.instance.SocketDragging(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item == null) return;
        fillCanvasGroup.alpha = 1f;
        if (!isDragSuccessful)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(InventoryView.instance.GetComponent<RectTransform>(), Input.mousePosition))
            {
                InventoryView.instance.SocketDroppedOnNothing(this);
            }
            else
            {
                InventoryView.instance.SocketDroppedOutside(this);
            }
            
        }
        draggedSocket = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSocket == null || draggedSocket.item == null) return;
        isDragSuccessful = true;
        InventoryView.instance.SocketDroppedOnSocket(draggedSocket, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (draggedSocket != null && draggedSocket.item != null) InventoryView.instance.SocketStartHoveringOverSocket(draggedSocket, this);
        else status = HighlightStatus.MouseOver;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        status = HighlightStatus.None;
    }

    private void StatusChanged()
    {
        overlay.color = InventoryView.instance.socketStatusOverlayColors[(int)status];
    }

    private void OnEnable()
    {
        status = HighlightStatus.None;
        fillCanvasGroup.alpha = 1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;
        if(eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            InventoryView.instance.SocketDefaultActionCalled(this);
        }
        else if (eventData.button== PointerEventData.InputButton.Right)
        {
            InventoryView.instance.SocketContextMenuCalled(this);
        }
    }
}
