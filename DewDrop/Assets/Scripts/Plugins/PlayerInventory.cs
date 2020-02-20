using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInventory : MonoBehaviour
{
    private LivingThing livingThing;

    public Consumable[] consumableBelt = new Consumable[5];
    public Equipment[] equipped = new Equipment[5];

    public List<Item> inventory = new List<Item>();
    public int inventoryCapacity = 6;


    private void Awake()
    {
        livingThing = GetComponent<LivingThing>();
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

        if (inventory.Count < inventoryCapacity)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Pickup(Item item)
    {
        Equipment equipment = item as Equipment;
        if (equipment != null && equipped[(int)equipment.type] == null)
        {
            inventory.Add(item);
            item.TransferOwnership(livingThing);
            EquipEquipmentFromInventory(inventory.Count - 1);
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
                    inventory.Add(item);
                    item.TransferOwnership(livingThing);
                    MoveConsumableFromInventoryToBelt(inventory.Count - 1,i);
                    SFXManager.CreateSFXInstance("si_local_ItemPickup", transform.position, true);
                    return true;
                }
            }
        }

        if (inventory.Count < inventoryCapacity)
        {
            inventory.Add(item);
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

    public void MoveConsumableFromBeltToInventory(int from, bool ignoreInventoryCapacity = false)
    {
        Consumable target = consumableBelt[from] as Consumable;
        if (target == null) return;
        if (!ignoreInventoryCapacity && inventory.Count >= inventoryCapacity) return;
        SFXManager.CreateSFXInstance("si_local_ItemUnequip", transform.position, true);
        inventory.Add(consumableBelt[from]);
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
                    inventory.RemoveAt(from);
                    SFXManager.CreateSFXInstance("si_local_ItemEquip", transform.position, true);
                    return true;
                }
            }
            return false;
        }
        else
        {
            inventory.RemoveAt(from);
            if (consumableBelt[to] != null)
            {
                inventory.Add(consumableBelt[to]);
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

        inventory.RemoveAt(from);

        if(equipped[(int)target.type] != null)
        {
            equipped[(int)target.type].Unequip();
            inventory.Add(equipped[(int)target.type]);
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
        if (!ignoreInventoryCapacity && inventory.Count >= inventoryCapacity) return;
        if (equipped[index] == null) return;
        inventory.Add(target);
        target.Unequip();
        equipped[index] = null;
        SFXManager.CreateSFXInstance("si_local_ItemUnequip", transform.position, true);
    }

    public void EquipGemFromInventory(int from, AbilityTrigger trigger)
    {
        Gem gem = inventory[from] as Gem;
        if (gem == null || gem.trigger != null || trigger.connectedGems.Count >= trigger.maxGems) return;
        
        inventory.RemoveAt(from);

        gem.Equip(trigger.name);
        //gem.OnEquip(livingThing, trigger);
    }

    public void UnequipGem(Gem gem, bool ignoreInventoryCapacity = false)
    {
        if (gem.trigger == null || (!ignoreInventoryCapacity && inventory.Count >= inventoryCapacity)) return;
        gem.Unequip();
        //gem.OnUnequip(livingThing, gem.trigger);
        inventory.Add(gem);
    }



}
