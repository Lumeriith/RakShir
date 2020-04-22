using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System;
using Doozy.Engine;

public class MapView : MonoBehaviour
{
    private Image mapNode;

    private Room[] rooms;
    private Image[] nodes;
    private NicerOutline[] outlines;
    private Color[] nodeColors;

    public Vector2 mapWorldSize = new Vector2(300f, 600f);

    private Vector2 minPoint;
    private Vector2 maxPoint;

    private RectTransform mapRect;

    public Color localPlayerNodeColor = Color.white;
    public Color enemyPlayerNodeColor = Color.red;
    public Color mapNodeColor;
    public Color obeliskMapNodeColor;
    private Color defaultNodeColor;

    private GameObject moveButton;


    private GameObject mapElements;
    private GameObject obeliskMapElements;

    private int selectedNode = -1;

    private void Awake()
    {
        mapNode = transform.Find("Window/Map Rect/Map Node").GetComponent<Image>();
        mapRect = transform.Find("Window/Map Rect").GetComponent<RectTransform>();
        moveButton = transform.Find("Window/Obelisk Map Elements/Move Button").gameObject;
        mapElements = transform.Find("Window/Map Elements").gameObject;
        obeliskMapElements = transform.Find("Window/Obelisk Map Elements").gameObject;
        mapNode.enabled = false;
    }

    private void OnEnable()
    {
        if (rooms == null) return;
        UpdateMap();

        if (GameManager.GetCurrentNode() == IngameNodeType.MapObelisk)
        {
            if (GameManager.instance.localPlayer.currentRoom.nextRooms.Count > 0)
            {
                selectedNode = Array.IndexOf(rooms, GameManager.instance.localPlayer.currentRoom.nextRooms[0]);
            }
            else
            {
                selectedNode = Array.IndexOf(rooms, GameManager.instance.localPlayer.currentRoom);
            }
        }
        else
        {
            selectedNode = Array.IndexOf(rooms, GameManager.instance.localPlayer.currentRoom);
        }

    }

    private void FixedUpdate()
    {
        if(GameManager.cachedCurrentNodeType == IngameNodeType.Map || GameManager.cachedCurrentNodeType == IngameNodeType.MapObelisk)
        {
            if(GameManager.cachedCurrentNodeType == IngameNodeType.MapObelisk)
            {
                defaultNodeColor = obeliskMapNodeColor;
                obeliskMapElements.SetActive(true);
                mapElements.SetActive(false);
            }
            else
            {
                defaultNodeColor = mapNodeColor;
                obeliskMapElements.SetActive(false);
                mapElements.SetActive(true);
            }
        }

        if (nodes == null || nodes.Length == 0)
        {
            UpdateMap();
            return;
        }
        int index;
        Relation relation;

        for (int i = 0; i < nodes.Length; i++)
        {
            nodeColors[i] = defaultNodeColor;
        }

        if (GladiatorGameManager.instance != null)
        {
            foreach (LivingThing player in GladiatorGameManager.instance.gamePlayers.Values)
            {
                index = Array.IndexOf(rooms, player.currentRoom);
                if (index != -1)
                {
                    relation = GameManager.instance.localPlayer.GetRelationTo(player);
                    if (relation == Relation.Own) nodeColors[index] = localPlayerNodeColor;
                    else if (relation == Relation.Enemy) nodeColors[index] = enemyPlayerNodeColor;
                    else nodeColors[index] = defaultNodeColor;
                }
            }
        }

        for(int i = 0; i < nodes.Length; i++)
        {
            nodes[i].color = nodeColors[i];
        }


        for(int i = 0; i < nodes.Length; i++)
        {
            if(i == selectedNode)
            {
                outlines[i].enabled = true;
            }
            else
            {
                outlines[i].enabled = false;
            }
        }

        if(selectedNode != -1)
        {
            moveButton.SetActive(GameManager.cachedCurrentNodeType == IngameNodeType.MapObelisk && GameManager.instance.localPlayer.currentRoom.nextRooms.Contains(rooms[selectedNode]));
        }
        else
        {
            moveButton.SetActive(false);
        }
    }

    private void UpdateMap()
    {
        print("Update Map");
        if(nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null) Destroy(nodes[i].gameObject);
            }
        }
        rooms = FindObjectsOfType<Room>();
        nodes = new Image[rooms.Length];
        outlines = new NicerOutline[rooms.Length];
        nodeColors = new Color[rooms.Length];
        minPoint = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        maxPoint = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        Image newNode;
        for (int i = 0; i < rooms.Length; i++)
        {
            newNode = Instantiate(mapNode.gameObject, mapRect).GetComponent<Image>();
            newNode.transform.position = new Vector2(-rooms[i].transform.position.x / mapWorldSize.x * mapRect.sizeDelta.x, -rooms[i].transform.position.z / mapWorldSize.y * mapRect.sizeDelta.y);
            if (minPoint.x > newNode.transform.position.x) minPoint.x = newNode.transform.position.x;
            if (minPoint.y > newNode.transform.position.y) minPoint.y = newNode.transform.position.y;
            if (maxPoint.x < newNode.transform.position.x) maxPoint.x = newNode.transform.position.x;
            if (maxPoint.y < newNode.transform.position.y) maxPoint.y = newNode.transform.position.y;
            newNode.enabled = true;
            int index = i;
            newNode.GetComponent<Button>().onClick.AddListener(()=> 
            {
                NodeClicked(index);
            });
            outlines[i] = newNode.GetComponent<NicerOutline>();
            outlines[i].enabled = false;
            nodes[i] = newNode;
        }
        Vector2 delta = (Vector2)mapRect.transform.position - (minPoint + maxPoint) / 2f;
        for (int i = 0; i < rooms.Length; i++)
        {
            nodes[i].transform.position += (Vector3)delta;
        }

    }

    private void NodeClicked(int index)
    {
        selectedNode = index;
    }

    public void Move()
    {
        GladiatorGameManager.DoObeliskTeleportation(rooms[selectedNode]);

    }

}
