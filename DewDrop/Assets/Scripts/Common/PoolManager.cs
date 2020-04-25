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

    public static GameObject GetPooledGameObjectFromPool(GameObject original, bool activate)
    {
        if (!_pools.TryGetValue(original, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _pools.Add(original, pool);
        }
        if (pool.Count == 0)
        {
            pool.Enqueue(Object.Instantiate(original, _poolRoot));
        }
        GameObject spawnling = pool.Dequeue();
        _originalBySpawnling.Add(spawnling, original);
        spawnling.SetActive(activate);
        return spawnling;
    }

    public static GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject spawnling = GetPooledGameObjectFromPool(original, true);
        spawnling.transform.position = position;
        spawnling.transform.rotation = rotation;
        spawnling.transform.parent = parent;

        return spawnling;
    }

    private static GameObject SpawnNoActivation(GameObject original, Vector3 position, Quaternion rotation)
    {
        GameObject spawnling = GetPooledGameObjectFromPool(original, false);
        spawnling.transform.position = position;
        spawnling.transform.rotation = rotation;

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
            gobj.transform.parent = GetPoolRoot();
            _pools[original].Enqueue(gobj);
            _originalBySpawnling.Remove(gobj);
        }
    }




}
