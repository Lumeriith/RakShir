using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public static class Dew
{
    public static IDewActionCaller DeserializeActionCaller(int serialized)
    {
        if (PhotonNetwork.PhotonViewExists(serialized))
        {
            AbilityInstanceSafeReference reference = AbilityInstanceSafeReference.RetrieveOrCreate(serialized);
            if (reference != null) return reference;

            PhotonView view = PhotonNetwork.GetPhotonView(serialized);
            return view.GetComponent<IDewActionCaller>();
        }
        return null;
    }
}
