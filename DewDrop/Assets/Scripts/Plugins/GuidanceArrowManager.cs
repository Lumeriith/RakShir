using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidanceArrowManager : MonoBehaviour
{
    private GameObject enemyIndicator;
    private GameObject objectiveIndicator;

    private List<Transform> enemyIndicators = new List<Transform>();

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


    private void Update()
    {
        if (GameManager.instance.localPlayer == null) return;
        transform.position = GameManager.instance.localPlayer.transform.position;
        transform.rotation = Quaternion.identity;

        List<LivingThing> enemies = new List<LivingThing>();

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



        for (int i = 0; i < enemies.Count; i++)
        {
            enemyIndicators[i].LookAt(enemies[i].transform);
            enemyIndicators[i].Rotate(90, 0, 0, Space.Self);
        }
    }
}
