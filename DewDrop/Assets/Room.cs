using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Room : MonoBehaviour
{

    public List<Room> nextRooms;
    public Transform entryPoint;

    [Header("Enemy Spawning Settings")]
    public List<Spawner> spawners;
    [Button("Add Stray Spawners")]
    private void AddStraySpawners()
    {
        Spawner[] spawners = GetComponentsInChildren<Spawner>();
        for (int i = 0; i < spawners.Length; i++)
        {
            if (!this.spawners.Contains(spawners[i])) this.spawners.Add(spawners[i]);
        }
    }

    public bool IsCleared()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            if(!spawners[i].IsCleared()) return false;
        }
        return true;
    }

    public void ActivateSpawners()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            spawners[i].Spawn();
        }
    }


}
