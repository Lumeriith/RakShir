using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerItemBelt : MonoBehaviour
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

    public bool Pickup(Item item)
    {
        Equipment equipment = item as Equipment;
        if (equipment != null && equipped[(int)equipment.type] == null)
        {
            inventory.Add(item);
            item.TransferOwnership(livingThing);
            EquipEquipmentFromInventory(inventory.Count - 1);
            return true;
        }

        Consumable consumable = item as Consumable;
        if(consumable != null && consumable.useOnPickup)
        {
            inventory.Add(item);
            item.TransferOwnership(livingThing);
            UseConsumable(consumable, new CastInfo());
            return true;
        }

        if (inventory.Count < inventoryCapacity)
        {
            inventory.Add(item);
            item.TransferOwnership(livingThing);
            return true;
        }
        else
        {
            return false;
        }
    }


    public void UseConsumable(Consumable consumable, CastInfo info)
    {
        for (int i = 0; i < consumableBelt.Length; i++)
        {
            if (consumableBelt[i] == consumable)
            {
                consumable.OnUse(info);
                break;
            }
        }
    }

    public void MoveConsumableFromBeltToInventory(int from)
    {
        Consumable target = consumableBelt[from] as Consumable;
        if (target == null) return;
        if (inventory.Count >= inventoryCapacity) return;
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
        return true;
    }
    
    public void UnequipEquipment(int index)
    {
        Equipment target = equipped[index] as Equipment;
        if (target == null) return;
        if (inventory.Count >= inventoryCapacity) return;
        if (equipped[index] == null) return;
        inventory.Add(target);
        target.Unequip();
        equipped[index] = null;
        
    }





}
