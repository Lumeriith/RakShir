using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PoolManager : MonoBehaviour, IPunPrefabPool
{
    public void Destroy(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }

    public new GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        
        throw new System.NotImplementedException();
    }
}
