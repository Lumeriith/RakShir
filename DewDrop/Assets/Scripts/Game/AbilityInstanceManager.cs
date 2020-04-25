using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public struct CastInfo
{
    public Entity owner;
    public Vector3 point;
    public Vector3 directionVector;
    public Entity target;

    public Quaternion directionQuaternion
    {
        get
        {
            return Quaternion.LookRotation(directionVector, Vector3.up);
        }
    }

    public static CastInfo OwnerOnly(Entity owner)
    {
        return new CastInfo { owner = owner };
    }

    public static CastInfo OwnerAndTarget(Entity owner, Entity target) {
        return new CastInfo { owner = owner, target = target };
    }
}


public class AbilityInstanceManager : MonoBehaviour
{

    public static AbilityInstance CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo, object[] data = null)
    {
        return CreateAbilityInstanceFromGem(prefabName, position, rotation, castInfo, null, data);
    }

    public static AbilityInstance CreateAbilityInstanceFromGem(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo, Gem gem, object[] data = null)
    {

        object[] initData;

        if (data == null)
        {
            initData = new object[5];
        }
        else
        {
            initData = new object[5 + data.Length];
        }

        if (castInfo.owner != null)
        {
            initData[0] = castInfo.owner.photonView.ViewID;
        }
        else
        {
            initData[0] = -1;
        }

        initData[1] = castInfo.point;
        initData[2] = castInfo.directionVector;

        if (castInfo.target != null)
        {
            initData[3] = castInfo.target.photonView.ViewID;
        }
        else
        {
            initData[3] = -1;
        }

        initData[4] = gem != null ? gem.photonView.ViewID : -1;

        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                initData[5 + i] = data[i];
            }
        }

        return PhotonNetwork.Instantiate("AbilityInstances/" + prefabName, position, rotation, 0, initData).GetComponent<AbilityInstance>();
    }
}
