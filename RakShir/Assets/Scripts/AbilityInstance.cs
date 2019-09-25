using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public abstract class AbilityInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    protected bool isCreated { get; private set; } = false;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] initData = info.photonView.InstantiationData;
        AbilityInstanceManager.CastInfo castInfo;
        if ((int)initData[0] != -1)
        {
            castInfo.owner = PhotonNetwork.GetPhotonView((int)initData[0]).GetComponent<LivingThing>();
        }
        else
        {
            castInfo.owner = null;
        }
        castInfo.point = (Vector3)initData[1];
        castInfo.directionVector = (Vector3)initData[2];

        if ((int)initData[3] != -1)
        {
            castInfo.target = PhotonNetwork.GetPhotonView((int)initData[3]).GetComponent<LivingThing>();
        }
        else
        {
            castInfo.target = null;
        }

        object[] data = new object[initData.Length - 4];
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = initData[i + 4];
        }

        isCreated = true;
        OnCreate(castInfo, data);
    }

    protected abstract void OnCreate(AbilityInstanceManager.CastInfo castInfo, object[] data);
}
