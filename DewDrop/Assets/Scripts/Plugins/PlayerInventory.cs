using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInventory : MonoBehaviour
{


    public LivingThing livingThing { private set; get; }

    public Consumable[] consumableBelt = new Consumable[3];
    public Equipment[] equipped = new Equipment[5];

    [HideInInspector]
    public Item[] inventory;
    public int inventoryCapacity = 12;

    private int inventoryFirstEmptyIndex
    {
        get
        {
            for(int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null) return i;
            }
            return -1;
        }
    }

    private bool isInventoryFull
    {
        get
        {
            for(int i = 0; i < inventoryCapacity; i++)
            {
                if (inventory[i] == null) return false;
            }
            return true;
        }
    }

    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
        inventory = new Item[inventoryCapacity + 1];
    }

    public bool HasSpaceFor(Item item)
    {
        Equipment equipment = item as Equipment;
        if (equipment != null && equipped[(int)equipment.type] == null)
        {
            return true;
        }

        Consumable consumable = item as Consumable;
        if (consumable != null && consumable.useOnPickup)
        {
            return true;
        }

        if (consumable != null)
        {
            for (int i = 0; i < consumableBelt.Length; i++)
            {
                if (consumableBelt[i] == null)
                {
                    return true;
                }
            }
        }

        return !isInventoryFull;
    }

    public bool Pickup(Item item)
    {
        Equipment equipment = item as Equipment;
        if (equipment != null && equipped[(int)equipment.type] == null)
        {
            int index = inventoryFirstEmptyIndex;
            inventory[inventoryFirstEmptyIndex] = item;
            item.TransferOwnership(livingThing);
            EquipEquipmentFromInventory(index);
            SFXManager.CreateSFXInstance("si_local_ItemPickup", transform.position, true);
            return true;
        }

        Consumable consumable = item as Consumable;
        if(consumable != null && consumable.useOnPickup)
        {
            item.TransferOwnership(livingThing);
            UseConsumable(consumable, new CastInfo { owner = livingThing });
            SFXManager.CreateSFXInstance("si_local_ItemPickup", transform.position, true);
            return true;
        }

        if(consumable != null)
        {
            for(int i = 0; i < consumableBelt.Length; i++)
            {
                if(consumableBelt[i] == null)
                {
                    int index = inventoryFirstEmptyIndex;
                    inventory[index] = item;
                    item.TransferOwnership(livingThing);
                    MoveConsumableFromInventoryToBelt(index, i);
                    SFXManager.CreateSFXInstance("si_local_ItemPickup", transform.position, true);
                    return true;
                }
            }
        }

        if (!isInventoryFull)
        {
            inventory[inventoryFirstEmptyIndex] = item;
            item.TransferOwnership(livingThing);
            SFXManager.CreateSFXInstance("si_local_ItemPickup", transform.position, true);
            return true;
        }
        else
        {
            return false;
        }
    }


    public void UseConsumable(Consumable consumable, CastInfo info)
    {
        consumable.OnUse(info);
        /*
        for (int i = 0; i < consumableBelt.Length; i++)
        {
            if (consumableBelt[i] == consumable)
            {
                consumable.OnUse(info);
                return;
            }
        }
        */

    }

    public void SwapBeltAndBelt(int from, int to)
    {
        Consumable cfrom = consumableBelt[from] as Consumable;
        Consumable cto = consumableBelt[to] as Consumable;

        consumableBelt[from] = cto;
        consumableBelt[to] = cfrom;
    }

    public void SwapInventoryAndBelt(int from, int to)
    {
        Consumable fromInventory = inventory[from] as Consumable;
        Consumable fromBelt = consumableBelt[to] as Consumable;

        if (inventory[from] != null && fromInventory == null) return; // Invalid action

        inventory[from] = fromBelt;
        consumableBelt[to] = fromInventory;
    }

    public void SwapInventoryAndEquipped(int from, int to)
    {
        Equipment fromInventory = inventory[from] as Equipment;
        Equipment fromEquipped = equipped[to];

        if (inventory[from] != null && fromInventory == null) return; // Invalid action (Inventory item is not an equipment)
        if (fromInventory != null && (int)fromInventory.type != to) return; // Invalid action (Equipment type mismatch)

        if(fromEquipped != null)
        {
            fromEquipped.Unequip();
        }

        if(fromInventory != null)
        {
            fromInventory.Equip();
        }

        inventory[from] = fromEquipped;
        equipped[to] = fromInventory;
    }

    public void SwapInventoryAndInventory(int from, int to)
    {
        Item temp = inventory[from];
        inventory[from] = inventory[to];
        inventory[to] = temp;
    }

    public void DropItemFromInventory(int from)
    {
        if (inventory[from] == null) return;
        inventory[from].Disown();
        inventory[from] = null;
    }

    public void DropItemFromBelt(int from)
    {
        if (consumableBelt[from] == null) return;
        consumableBelt[from].Disown();
        consumableBelt[from] = null;
    }

    public void DropItemFromEquipped(int from)
    {
        if (equipped[from] == null) return;
        equipped[from].Unequip();
        equipped[from].Disown();
        equipped[from] = null;
    }


    public void MoveConsumableFromBeltToInventory(int from, bool ignoreInventoryCapacity = false)
    {
        Consumable target = consumableBelt[from] as Consumable;
        if (target == null) return;
        if (!ignoreInventoryCapacity && isInventoryFull) return;
        SFXManager.CreateSFXInstance("si_local_ItemUnequip", transform.position, true);
        inventory[inventoryFirstEmptyIndex] = consumableBelt[from];
        consumableBelt[from] = null;
    }

    public bool MoveConsumableFromInventoryToBelt(int from, int to = -1)
    {
        Consumable target = inventory[from] as Consumable;
        if (target == null) return false;
        if (to == -1)
        {
            for (int i = 0; i < consumableBelt.Length; i++)
            {
                if (consumableBelt[i] == null)
                {
                    consumableBelt[i] = target;
                    inventory[from] = null;
                    SFXManager.CreateSFXInstance("si_local_ItemEquip", transform.position, true);
                    return true;
                }
            }
            return false;
        }
        else
        {
            inventory[from] = null;
            if (consumableBelt[to] != null)
            {
                inventory[inventoryFirstEmptyIndex] = consumableBelt[to];
            }
            consumableBelt[to] = target;
            SFXManager.CreateSFXInstance("si_local_ItemEquip", transform.position, true);
            return true;
        }
    }
    public bool EquipEquipmentFromInventory(int from)
    {
        Equipment target = inventory[from] as Equipment;
        if (target == null) return false;

        inventory[from] = null;

        if(equipped[(int)target.type] != null)
        {
            equipped[(int)target.type].Unequip();
            inventory[inventoryFirstEmptyIndex] = equipped[(int)target.type];
        }

        equipped[(int)target.type] = target;
        equipped[(int)target.type].Equip();
        SFXManager.CreateSFXInstance("si_local_ItemEquip", transform.position, true);
        return true;
    }
    
    public void UnequipEquipment(int index, bool ignoreInventoryCapacity = false)
    {
        Equipment target = equipped[index] as Equipment;
        if (target == null) return;
        if (!ignoreInventoryCapacity && isInventoryFull) return;
        if (equipped[index] == null) return;
        inventory[inventoryFirstEmptyIndex] = target;
        target.Unequip();
        equipped[index] = null;
        SFXManager.CreateSFXInstance("si_local_ItemUnequip", transform.position, true);
    }

    public void EquipGemFromInventory(int from, AbilityTrigger trigger)
    {
        Gem gem = inventory[from] as Gem;
        if (gem == null || gem.trigger != null || trigger.connectedGems.Count >= AbilityTrigger.maxGemPerTrigger) return;
        
        inventory[from] = null;

        gem.Equip(trigger.name);
        //gem.OnEquip(livingThing, trigger);
    }

    public void UnequipGem(Gem gem, bool ignoreInventoryCapacity = false)
    {
        if (gem.trigger == null || (!ignoreInventoryCapacity && isInventoryFull)) return;
        gem.Unequip();
        //gem.OnUnequip(livingThing, gem.trigger);
        inventory[inventoryFirstEmptyIndex] = gem;
    }

    public void UnequipAllGemsInTrigger(AbilityTrigger trigger)
    {
        Gem gem;
        for(int i = trigger.connectedGems.Count - 1; i >=0; i--)
        {
            if (isInventoryFull) break;
            gem = trigger.connectedGems[i];
            gem.Unequip();
            inventory[inventoryFirstEmptyIndex] = gem;
        }
    }



}
