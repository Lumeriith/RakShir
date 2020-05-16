using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidanceArrowManager : MonoBehaviour
{
    public static GuidanceArrowManager instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<GuidanceArrowManager>();
            return _instance;
        }
    }
    private static GuidanceArrowManager _instance;

    private GameObject enemyIndicator;
    private GameObject objectiveIndicator;

    private List<Transform> enemyIndicators = new List<Transform>();

    private bool isObjectiveSet;
    private Vector3 objectivePosition;

    public float minimumDistanceToShowEnemyArrow = 15f;

    public Vector2 objectiveIndicatorSize = new Vector2(2f, 5f);
    public Vector2 objectiveIndicatorDistance = new Vector2(5f, 20f);

    


    private void Awake()
    {
        enemyIndicator = transform.Find("Enemy Indicator").gameObject;
        objectiveIndicator = transform.Find("Objective Indicator").gameObject;
        enemyIndicator.SetActive(false);
        objectiveIndicator.SetActive(false);
    }

    public static void SetObjective(Vector3 position)
    {
        instance.objectivePosition = position;
        instance.isObjectiveSet = true;
    }

    public static void RemoveObjective()
    {
        instance.isObjectiveSet = false;
    }


    private void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        transform.position = GameManager.instance.localPlayer.transform.position;
        transform.rotation = Quaternion.identity;

        List<Entity> enemies = new List<Entity>();

        for(int i = 0; i < GameManager.instance.everyLivingThings.Count; i++)
        {
            if(GameManager.instance.localPlayer.GetRelationTo(GameManager.instance.everyLivingThings[i]) == Relation.Enemy &&
                GameManager.instance.everyLivingThings[i].IsAlive() &&
                GameManager.instance.everyLivingThings[i].currentRoom == GameManager.instance.localPlayer.currentRoom &&
                Vector3.Distance(GameManager.instance.everyLivingThings[i].transform.position, GameManager.instance.localPlayer.transform.position) > minimumDistanceToShowEnemyArrow) enemies.Add(GameManager.instance.everyLivingThings[i]);
        }


        while (enemyIndicators.Count > enemies.Count)
        {
            Destroy(enemyIndicators[enemyIndicators.Count - 1].gameObject);
            enemyIndicators.RemoveAt(enemyIndicators.Count - 1);
        }

        while (enemyIndicators.Count < enemies.Count)
        {
            enemyIndicators.Add(Instantiate(enemyIndicator, transform.position, transform.rotation, transform).transform);
            enemyIndicators[enemyIndicators.Count - 1].gameObject.SetActive(true);
        }


        Vector3 pos;
        for (int i = 0; i < enemies.Count; i++)
        {
            pos = enemies[i].transform.position;
            pos.y = enemyIndicators[i].position.y;
            enemyIndicators[i].LookAt(pos);
            enemyIndicators[i].Rotate(90, 0, 0, Space.Self);
        }

        objectiveIndicator.SetActive(isObjectiveSet);
        objectivePosition.y = transform.position.y;
        objectiveIndicator.transform.LookAt(objectivePosition);
        
        objectiveIndicator.transform.Rotate(90, 0, 0, Space.Self);
        objectiveIndicator.transform.localScale =
            Vector3.one
            * (Mathf.Clamp(Vector3.Distance(GameManager.instance.localPlayer.transform.position, objectivePosition), objectiveIndicatorDistance.x, objectiveIndicatorDistance.y)
            / (objectiveIndicatorDistance.y - objectiveIndicatorDistance.x) * (objectiveIndicatorSize.y - objectiveIndicatorSize.x) + objectiveIndicatorSize.x);
    }
}
