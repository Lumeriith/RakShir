using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerItemBelt : MonoBehaviour
{
    public Consumable[] consumables = new Consumable[6];

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
        for(int i = 0; i < consumables.Length; i++)
        {
            if(consumables[i] == null)
            {
                consumables[i] = consumable;
                return true;
            }
        }

        return false;
    }
}
