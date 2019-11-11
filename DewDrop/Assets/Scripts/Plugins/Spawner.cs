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

    private bool started = false;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
    }

    public bool IsCleared()
    {
        if (spawnedLivingThings.Count == 0) return started;
        for (int i = 0; i < spawnedLivingThings.Count; i++)
        {
            if (spawnedLivingThings[i] != null && spawnedLivingThings[i].IsAlive()) return false;
        }
        return true;
    }


    public void Spawn(LivingThing activator = null)
    {
        started = true;
        LivingThing thing = PhotonNetwork.Instantiate(livingThingPrefab.name, transform.position, transform.rotation).GetComponent<LivingThing>();
        spawnedLivingThings.Add(thing);
        if(activator != null)
        {
            thing.control.CommandChase(activator);
        }
    }

    public void DestroyAllSpawnedLivingThings()
    {
        for (int i = 0; i < spawnedLivingThings.Count; i++)
        {
            if (spawnedLivingThings[i] != null) spawnedLivingThings[i].Destroy();
        }

        spawnedLivingThings.Clear();
    }



}
