using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PoolManager : MonoBehaviour
{
    private static Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private static Dictionary<GameObject, GameObject> _originalBySpawnling = new Dictionary<GameObject, GameObject>();
    private static Transform _poolRoot = null;

    public static Transform GetPoolRoot()
    {
        if(_poolRoot == null)
        {
            _poolRoot = new GameObject("Pool Root").transform;
            _poolRoot.SetAsFirstSibling();
        }
        return _poolRoot;
    }


    public static GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!_pools.TryGetValue(original, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _pools.Add(original, pool);
        }

        GameObject spawnling;

        if (pool.Count == 0)
        {
            spawnling = Instantiate(original, position, rotation, parent);
        }
        else
        {
            spawnling = pool.Dequeue();
            spawnling.transform.position = position;
            spawnling.transform.rotation = rotation;
            spawnling.transform.SetParent(parent, true);
        }
        _originalBySpawnling.Add(spawnling, original);
        spawnling.SetActive(true);
        return spawnling;
    }

    public static void Despawn(GameObject gobj)
    {
        if(!_originalBySpawnling.TryGetValue(gobj, out GameObject original))
        {
            Debug.LogErrorFormat("Tried to despawn a GameObject with unknown original! Destroying it instead. {0}", gobj);
            Object.Destroy(gobj);
        }
        else
        {
            gobj.SetActive(false);
            gobj.transform.SetParent(GetPoolRoot(), false);
            _pools[original].Enqueue(gobj);
            _originalBySpawnling.Remove(gobj);
        }
    }




}
