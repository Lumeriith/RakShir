using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapElementMonsterSpawner : MapElement
{
    public List<GameObject> monstersToSpawn;
    public float radius = 3f;
    public bool lockUntilAllDead = true;
    public float spawnDelay = 0.3f;
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = new Color(1, 0, 0, 0.05f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, radius);
    }
	#endif
    protected override void OnActivate(bool isLocal)
    {
        if (!isLocal) return;
        StartCoroutine(CoroutineSpawnEnemies());
    }

    private IEnumerator CoroutineSpawnEnemies()
    {
        List<LivingThing> spawnedMonsters = new List<LivingThing>();
        Vector3 position;
        for(int i = 0; i < monstersToSpawn.Count; i++)
        {
            position = Random.insideUnitCircle;
            position.z = position.y;
            position.y = 0f;
            position = position * radius + transform.position;
            spawnedMonsters.Add(GameManager.SpawnLivingThing(monstersToSpawn[i].name, position));
            spawnedMonsters[spawnedMonsters.Count - 1].control.CommandChase(GameManager.instance.localPlayer);
            yield return new WaitForSeconds(spawnDelay);
        }

        if (lockUntilAllDead)
        {
            bool isAllDead = false;
            while (!isAllDead)
            {
                isAllDead = true;
                for(int i = 0; i < spawnedMonsters.Count; i++)
                {
                    if (spawnedMonsters[i].IsAlive())
                    {
                        isAllDead = false;
                        break;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            MarkAsFinished();
        }
        else
        {
            MarkAsFinished();
        }
    }
}
