using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
public class Spawner : MonoBehaviour
{
    [ShowAssetPreview]
    public GameObject livingThingPrefab;


    [HideInInspector]
    public List<LivingThing> spawnedLivingThings;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
    }

    public bool IsCleared()
    {
        if (spawnedLivingThings.Count == 0) return false;
        for (int i = 0; i < spawnedLivingThings.Count; i++)
        {
            if (spawnedLivingThings[i] == null || spawnedLivingThings[i].IsAlive()) return false;
        }
        return true;
    }


    public void Spawn()
    {
        spawnedLivingThings.Add(PhotonNetwork.Instantiate(livingThingPrefab.name, transform.position, transform.rotation).GetComponent<LivingThing>());
    }

    public void DestroyAllSpawnedLivingThings()
    {
        for (int i = 0; i < spawnedLivingThings.Count; i++)
        {
            PhotonNetwork.Destroy(spawnedLivingThings[i].gameObject);
        }

        spawnedLivingThings.Clear();
    }



}
