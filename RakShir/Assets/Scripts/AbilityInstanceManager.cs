using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public struct CastInfo
{
    public LivingThing owner;
    public Vector3 point;
    public Vector3 directionVector;
    public LivingThing target;

    public Quaternion directionQuaternion
    {
        get
        {
            return Quaternion.LookRotation(directionVector, Vector3.up);
        }
    }
}
public class AbilityInstanceManager : MonoBehaviour
{

    public static void CreateAbilityInstance(string prefabName, Vector3 position, Quaternion rotation, CastInfo castInfo = new CastInfo(), object[] data = null)
    {
        object[] initData;

        if (data == null)
        {
            initData = new object[4];
        }
        else
        {
            initData = new object[4 + data.Length];
        }
        
        if(castInfo.owner != null)
        {
            initData[0] = castInfo.owner.photonView.ViewID;
        }
        else
        {
            initData[0] = -1;
        }

        initData[1] = castInfo.point;
        initData[2] = castInfo.directionVector;
        
        if(castInfo.target != null)
        {
            initData[3] = castInfo.target.photonView.ViewID;
        }
        else
        {
            initData[3] = -1;
        }

        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                initData[4 + i] = data[i];
            }
        }
        PhotonNetwork.Instantiate(prefabName, position, rotation, 0, initData);
    }

   
}
