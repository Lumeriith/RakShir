using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Spawner : MonoBehaviour
{
    public GameObject livingThingPrefab;


    [HideInInspector]
    public List<Entity> spawnedLivingThings;

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


    public void Spawn(Entity activator = null)
    {
        started = true;
        Entity thing = Dew.SpawnEntity(livingThingPrefab.name, transform.position, transform.rotation);
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
