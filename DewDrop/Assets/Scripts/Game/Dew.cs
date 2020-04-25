using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public static class Dew
{
    public static LivingThing SpawnEntity(string livingThingName, Vector3 location, Quaternion rotation = new Quaternion())
    {
        return PhotonNetwork.Instantiate("Entities/" + livingThingName, location, rotation).GetComponent<LivingThing>();
    }

    public static Item SpawnItem(string itemName, Vector3 location, Quaternion rotation = new Quaternion())
    {
        return PhotonNetwork.Instantiate("Items/" + itemName, location, rotation).GetComponent<Item>();
    }


}
