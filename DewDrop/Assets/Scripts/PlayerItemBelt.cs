using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerItemBelt : MonoBehaviour
{
    public Consumable[] consumables = new Consumable[6];
    public Equippable[] equippables = new Equippable[3];

    public Equippable[] equipped = new Equippable[5];

    public void UseConsumable(Consumable consumable, CastInfo info)
    {
        for (int i = 0; i < consumables.Length; i++)
        {
            if (consumables[i] == consumable)
            {
                bool isUsed = consumable.OnUse(info);
                if (isUsed)
                {
                    PhotonNetwork.Destroy(consumable.photonView);
                    consumables[i] = null;
                }
                break;
            }
        }

    }

    public bool AddConsumable(Consumable consumable)
    {
        for (int i = 0; i < consumables.Length; i++)
        {
            if (consumables[i] == null)
            {
                consumables[i] = consumable;
                return true;
            }
        }

        return false;
    }
    public bool AddEquippable(Equippable equippable)
    {
        for (int i = 0; i < equippables.Length; i++)
        {
            if (equippables[i] == null)
            {
                equippables[i] = equippable;
                if(equipped[(int)equippable.type] == null)
                {
                    UseEquippable(equippable);
                }
                return true;
            }
        }

        return false;
    }

    public void UseEquippable(Equippable equippable)
    {
        for (int i = 0; i < equippables.Length; i++)
        {
            if(equipped[(int)equippable.type] == null)
            {
                equipped[(int)equippable.type] = equippable;
                equippables[i] = null;
                equippable.Equip();
            }
            else
            {
                equipped[(int)equippable.type].Unequip();
                equippables[i] = equipped[(int)equippable.type];
                equipped[(int)equippable.type] = equippable;
                equippable.Equip();
            }
        }

    }




}
